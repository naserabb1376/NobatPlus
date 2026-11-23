using System.Data;
using System.Text.Json;
using Domains;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Services;

public class BillingRep : IBillingRep
{
    private readonly NobatPlusContext _context;
    public BillingRep(NobatPlusContext context) => _context = context;

    public async Task<bool> CanAccessPaymentAsync(long paymentId, long personId, bool isAdmin)
    {
        if (isAdmin) return true;
        if (personId <= 0) return false;
        return await _context.Payments.AnyAsync(p => p.ID == paymentId &&
            (p.OwnerPersonID == personId || (p.OwnerPersonID == null && p.PaymentBookings.Any() &&
                p.PaymentBookings.All(b => b.Booking.Customer.PersonID == personId))));
    }

    public Task<BitResultObject> CreateGatewayAttemptAsync(long paymentId, string gatewayName, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        int reqExpireDuration = 1;
        if (!await CanAccessPaymentAsync(paymentId, personId, isAdmin)) throw new InvalidOperationException("دسترسی به پرداخت مجاز نیست");
        var payment = await _context.Payments.SingleAsync(x => x.ID == paymentId);
        if (!payment.IsActive || payment.PaymentFinished || payment.PaymentStatus == "refunded") throw new InvalidOperationException("پرداخت قابل انجام نیست");
        if (payment.Purpose == PaymentPurposes.Service)
        {
            // Legacy service creation stored a planned deposit as PayedAmount.
            // Successful history rows are the source of truth when requesting payment.
            payment.PayedAmount = await _context.PaymentHistories.Where(x => x.PaymentID == payment.ID && x.PaymentStatus).SumAsync(x => x.Amount);
            payment.RemainAmount = Math.Max(0, payment.AllPaymentAmount - payment.PayedAmount);
        }
        if (payment.CurrencyCode != "IRR") throw new InvalidOperationException("واحد پول درگاه باید IRR باشد");
        var pending = await _context.PaymentHistories.Where(x => x.PaymentID == paymentId && !x.PaymentStatus
            && (x.GatewayRequestState == "creating" || x.GatewayRequestState == "requested") && x.RequestExpiresAt > DateTime.UtcNow).AnyAsync();
        if (pending) throw new InvalidOperationException($"یک درخواست پرداخت فعال وجود دارد؛ همان لینک را ادامه دهید یا {reqExpireDuration} دقیقه صبر کنید");
        var remaining = Math.Max(0, payment.AllPaymentAmount - payment.PayedAmount);
        var amount = payment.Purpose == PaymentPurposes.Service && payment.PayedAmount == 0 && payment.DepositAmount > 0
            ? Math.Min(payment.DepositAmount, remaining) : remaining;
        Money(amount);
        var history = new PaymentHistory
        {
            PaymentID = payment.ID, Amount = amount, PaymentMethod = payment.PaymentLevel, PaymentStatus = false,
            GatewayName = gatewayName, GatewayRequestState = "creating", RequestExpiresAt = DateTime.UtcNow.AddMinutes(reqExpireDuration),
            PaymentDate = DateTime.Now, CreateDate = DateTime.Now, UpdateDate = DateTime.Now
        };
        _context.PaymentHistories.Add(history);
        await _context.SaveChangesAsync();
        return history.ID;
    });

    public Task<BitResultObject> SaveGatewayAttemptAsync(long historyId, bool success, string? url, string? message) => WriteAsync(async () =>
    {
        var row = await _context.PaymentHistories.SingleAsync(x => x.ID == historyId);
        if (!row.PaymentStatus)
        {
            row.GatewayRequestState = success ? "requested" : "failed";
            row.GatewayUrl = url; row.GatewayMessage = message; row.UpdateDate = DateTime.Now;
        }
        return row.ID;
    });

    public async Task<RowResultObject<PaymentVerificationContext>> GetPaymentVerificationContextAsync(long historyId)
    {
        var result = new RowResultObject<PaymentVerificationContext>();
        try
        {
            var row = await _context.PaymentHistories
                .AsNoTracking()
                .Where(x => x.ID == historyId)
                .Select(x => new
                {
                    HistoryID = x.ID,
                    PaymentID = x.PaymentID,
                    Purpose = x.Payment.Purpose,
                    BookingIDs = x.Payment.PaymentBookings.Select(pb => pb.BookingID).ToList()
                })
                .SingleOrDefaultAsync();

            if (row == null)
            {
                result.Status = false;
                result.ErrorMessage = "سابقه پرداخت یافت نشد";
                return result;
            }

            string? profileType = null;
            if (row.Purpose.Equals(PaymentPurposes.Subscription, StringComparison.OrdinalIgnoreCase))
            {
                var businessId = await _context.BusinessSubscriptions
                    .AsNoTracking()
                    .Where(x => x.PaymentID == row.PaymentID)
                    .Select(x => (long?)x.BusinessID)
                    .SingleOrDefaultAsync();

                if (businessId.HasValue)
                {
                    var isWorkshop = await _context.Workers
                        .AsNoTracking()
                        .Where(x => x.ID == businessId.Value)
                        .Select(x => (bool?)x.IsWorkShop)
                        .SingleOrDefaultAsync();
                    profileType = isWorkshop == true ? "Workshop" : isWorkshop == false ? "Worker" : null;
                }
            }
            else if (row.Purpose.Equals(PaymentPurposes.Service, StringComparison.OrdinalIgnoreCase))
            {
                profileType = "Customer";
            }

            result.Result = new PaymentVerificationContext
            {
                PaymentHistoryID = row.HistoryID,
                PaymentID = row.PaymentID,
                Purpose = row.Purpose,
                ProfileType = profileType,
                BookingIDs = row.BookingIDs
            };
        }
        catch (Exception ex)
        {
            result.Status = false;
            result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
        }

        return result;
    }

    // One short database lock serializes ledger writes across API instances and Hangfire.
    // Gateway network requests are deliberately outside this transaction.
    public async Task AcquireLockAsync()
    {
        if (_context.Database.CurrentTransaction == null)
            throw new InvalidOperationException("Financial writes require a database transaction.");
        await _context.Database.ExecuteSqlRawAsync("DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource = N'Nobatix:Billing', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000; IF @r < 0 THROW 51001, 'Financial lock timeout. Retry the request.', 1;");
    }

    private async Task<BitResultObject> WriteAsync(Func<Task<long>> action)
    {
        var result = new BitResultObject();
        try
        {
            if (_context.Database.CurrentTransaction != null)
            {
                await AcquireLockAsync();
                result.ID = await action();
                await _context.SaveChangesAsync();
            }
            else
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                await AcquireLockAsync();
                result.ID = await action();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }
        catch (Exception ex)
        {
            _context.ChangeTracker.Clear();
            result.Status = false;
            result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
        }
        return result;
    }

    private async Task<Worker> RequireWorkerAsync(long id, long personId, bool admin)
    {
        var worker = await _context.Workers.AsNoTracking().SingleOrDefaultAsync(x => x.ID == id);
        if (worker == null || (!admin && worker.PersonID != personId))
            throw new InvalidOperationException("دسترسی به پروفایل مالی مجاز نیست");
        return worker;
    }

    private static bool HasApprovedAccount(Worker worker) => worker.AccountStatus == "1";

    private async Task<bool> IsEligibleProviderAsync(long workerId, long businessId)
    {
        return await _context.Workers.AsNoTracking()
            .AnyAsync(x => x.ID == workerId && (x.WorkerParentID == businessId || (x.ID == businessId && x.WorkerParentID == 0)) && !x.IsWorkShop &&
                x.AccountStatus == "1" && x.Person.IsActive && x.WorkerServices.Any());
    }

    private Task<int> GetConsumedMemberCountAsync(long businessId, string planType)
    {
        var query = _context.Workers.AsNoTracking().Where(x =>
            ((x.WorkerParentID == businessId && !x.IsWorkShop) || (x.ID == businessId && x.WorkerParentID == 0 && !x.IsWorkShop)) && x.AccountStatus == "1" &&
            x.Person.IsActive && x.WorkerServices.Any());

        if (string.Equals(planType, "Individual", StringComparison.OrdinalIgnoreCase))
            query = query.Where(x => x.ID == businessId && x.WorkerParentID == 0);

        return query.Select(x => x.ID).Distinct().CountAsync();
    }

    private Task<int> GetRegisteredMemberCountAsync(long businessId, string planType)
    {
        if (string.Equals(planType, "Individual", StringComparison.OrdinalIgnoreCase))
            return _context.Workers.AsNoTracking().CountAsync(x => x.ID == businessId && !x.IsWorkShop && x.WorkerParentID == 0);

        return _context.Workers.AsNoTracking().CountAsync(x => x.WorkerParentID == businessId && !x.IsWorkShop);
    }

    private async Task<BillingAccount> AccountAsync(long workerId)
    {
        var account = await _context.BillingAccounts.SingleOrDefaultAsync(x => x.WorkerID == workerId);
        if (account != null) return account;
        var worker = await _context.Workers.AsNoTracking().SingleAsync(x => x.ID == workerId);
        account = new BillingAccount { WorkerID = workerId, OwnerType = worker.IsWorkShop ? "Business" : "Worker", CreateDate = DateTime.UtcNow };
        _context.BillingAccounts.Add(account);
        await _context.SaveChangesAsync();
        return account;
    }

    private static string Choice(string? value, params string[] allowed) =>
        allowed.FirstOrDefault(x => string.Equals(x, value?.Trim(), StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException("مقدار تنظیمات مالی معتبر نیست");

    private static void Money(decimal amount, bool allowZero = false)
    {
        if (amount < 0 || (!allowZero && amount == 0) || amount != decimal.Truncate(amount) || amount > 9999999999999999m)
            throw new InvalidOperationException("مبلغ باید عدد صحیح مثبت و بر حسب ریال باشد");
    }

    public Task<BitResultObject> SavePlanAsync(SubscriptionPlan plan, bool isAdmin) => WriteAsync(async () =>
    {
        if (!isAdmin) throw new InvalidOperationException("فقط مدیر مجاز به تغییر پلن است");
        if (string.IsNullOrWhiteSpace(plan.Name) || plan.Name.Trim().Length > 200 || !new[] { 1, 3, 6, 12 }.Contains(plan.DurationMonths))
            throw new InvalidOperationException("نام و مدت پلن معتبر نیست");
        var planType = new[] { "Individual", "Business" }.FirstOrDefault(x =>
            string.Equals(x, plan.PlanType?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (planType == null)
            throw new InvalidOperationException("نوع پلن باید Individual یا Business باشد");
        if (plan.MaxMemberCount < 1)
            throw new InvalidOperationException("حداکثر تعداد اعضای پلن باید حداقل یک نفر باشد");
        Money(plan.Price); Money(plan.BookingFeeAmount, true);
        if (!string.Equals(plan.CurrencyCode, "IRR", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("واحد پول این نسخه ریال (IRR) است");
        var row = plan.ID > 0 ? await _context.SubscriptionPlans.SingleAsync(x => x.ID == plan.ID) : new SubscriptionPlan { CreateDate = DateTime.UtcNow };
        row.Name = plan.Name.Trim(); row.DurationMonths = plan.DurationMonths; row.Price = plan.Price;
        row.PlanType = planType; row.MaxMemberCount = plan.MaxMemberCount;
        row.BookingFeeAmount = plan.BookingFeeAmount; row.CurrencyCode = "IRR";
        row.ContractVersion = plan.ContractVersion.Trim(); row.IsActive = plan.IsActive;
        row.Description = plan.Description; row.UpdateDate = DateTime.UtcNow;
        if (row.ID == 0) _context.SubscriptionPlans.Add(row);
        await _context.SaveChangesAsync();
        return row.ID;
    });

    public Task<BitResultObject> SaveBusinessSettingsAsync(BusinessBillingSetting setting, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        var business = await RequireWorkerAsync(setting.BusinessID, personId, isAdmin);
        if (business.WorkerParentID > 0 && !business.IsWorkShop) throw new InvalidOperationException("تنظیمات کسب‌وکار باید برای پروفایل سالن یا متخصص مستقل ثبت شود");
        setting.DefaultFeePayer = Choice(setting.DefaultFeePayer, "Business", "Worker");
        setting.CollectionMode = Choice(setting.CollectionMode, "Prepaid", "Postpaid", "Hybrid");
        setting.InvoiceCycle = Choice(setting.InvoiceCycle, "Weekly", "Monthly");
        setting.SettlementDestination = Choice(setting.SettlementDestination, "Business", "Separate");
        Money(setting.CreditLimit, true);
        if (setting.DueDays < 0 || setting.DueDays > 365) throw new InvalidOperationException("مهلت صورتحساب معتبر نیست");
        var row = await _context.BusinessBillingSettings.SingleOrDefaultAsync(x => x.BusinessID == setting.BusinessID);
        var before = JsonSerializer.Serialize(row);
        if (row == null)
        {
            row = new BusinessBillingSetting { BusinessID = setting.BusinessID, CreateDate = DateTime.UtcNow, EffectiveFrom = DateTime.UtcNow };
            _context.BusinessBillingSettings.Add(row);
        }
        if (!row.IsBookingFeeEnabled && setting.IsBookingFeeEnabled) row.EffectiveFrom = DateTime.UtcNow;
        row.DefaultFeePayer = setting.DefaultFeePayer; row.CollectionMode = setting.CollectionMode;
        row.InvoiceCycle = setting.InvoiceCycle; row.SettlementDestination = setting.SettlementDestination;
        row.IsBookingFeeEnabled = setting.IsBookingFeeEnabled;
        row.CreditLimit = setting.CreditLimit; row.DueDays = setting.DueDays;
        row.IsActive = setting.IsActive; row.UpdateDate = DateTime.UtcNow;
        Audit(setting.BusinessID, null, personId, before, row);
        await AccountAsync(setting.BusinessID);
        await _context.SaveChangesAsync();
        return row.ID;
    });

    private void Audit(long businessId, long? workerId, long actor, string before, object after) =>
        _context.BillingSettingHistories.Add(new BillingSettingHistory
        {
            BusinessID = businessId, WorkerID = workerId, ActorPersonID = actor,
            BeforeJson = before, AfterJson = JsonSerializer.Serialize(after), OccurredAt = DateTime.UtcNow, CreateDate = DateTime.UtcNow
        });

    public Task<BitResultObject> SaveWorkerSettingsAsync(long businessId, long workerId, string? payer, decimal? revenueSharePercent, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        await RequireWorkerAsync(businessId, personId, isAdmin);
        var worker = await _context.Workers.AsNoTracking().SingleAsync(x => x.ID == workerId);
        if (worker.ID != businessId && worker.WorkerParentID != businessId)
            throw new InvalidOperationException("متخصص عضو این کسب‌وکار نیست");
        if (worker.IsWorkShop || worker.WorkerParentID != businessId)
            throw new InvalidOperationException("تنظیم سهم فقط برای متخصص عضو مجموعه قابل ثبت است");
        if (revenueSharePercent.HasValue && (revenueSharePercent.Value < 0 || revenueSharePercent.Value > 100))
            throw new InvalidOperationException("درصد سهم مجموعه باید بین صفر و صد باشد");
        var row = await _context.WorkerBillingSettings.SingleOrDefaultAsync(x => x.BusinessID == businessId && x.WorkerID == workerId);
        var before = JsonSerializer.Serialize(row);
        if (row == null)
        {
            row = new WorkerBillingSetting { BusinessID = businessId, WorkerID = workerId, CreateDate = DateTime.UtcNow };
            _context.WorkerBillingSettings.Add(row);
        }
        row.FeePayerOverride = string.IsNullOrWhiteSpace(payer) ? null : Choice(payer, "Business", "Worker");
        if (revenueSharePercent.HasValue) row.RevenueSharePercent = revenueSharePercent.Value;
        row.UpdateDate = DateTime.UtcNow;
        Audit(businessId, workerId, personId, before, row);
        await _context.SaveChangesAsync();
        return row.ID;
    });

    public Task<BitResultObject> AcceptWorkerLiabilityAsync(long businessId, long workerId, bool accept, long personId, string? contractVersion = null) => WriteAsync(async () =>
    {
        var worker = await RequireWorkerAsync(workerId, personId, false);
        if (worker.ID != businessId && worker.WorkerParentID != businessId) throw new InvalidOperationException("ارتباط کسب‌وکار معتبر نیست");
        var row = await _context.WorkerBillingSettings.SingleOrDefaultAsync(x => x.BusinessID == businessId && x.WorkerID == workerId);
        var before = JsonSerializer.Serialize(row);
        if (row == null)
        {
            row = new WorkerBillingSetting { BusinessID = businessId, WorkerID = workerId, CreateDate = DateTime.UtcNow };
            _context.WorkerBillingSettings.Add(row);
        }
        row.AcceptedByPersonID = accept ? personId : null; row.AcceptedAt = accept ? DateTime.UtcNow : null;
        row.AcceptedContractVersion = accept ? await _context.BusinessSubscriptions
            .Where(x => x.BusinessID == businessId && x.Status == "active" && x.StartsAt <= DateTime.UtcNow && x.EndsAt > DateTime.UtcNow)
            .Select(x => x.ContractVersionSnapshot).FirstOrDefaultAsync() : null;
        if (accept && row.AcceptedContractVersion == null) throw new InvalidOperationException("برای پذیرش قرارداد، اشتراک فعال لازم است");
        if (accept && row.AcceptedContractVersion != contractVersion) throw new InvalidOperationException("نسخه قرارداد تغییر کرده است؛ شرایط فعلی را مجدداً مشاهده و تایید کنید");
        row.UpdateDate = DateTime.UtcNow;
        Audit(businessId, workerId, personId, before, row);
        await _context.SaveChangesAsync();
        return row.ID;
    });

    public async Task<RowResultObject<WorkerBillingContext>> GetWorkerBillingContextAsync(long workerId, long personId, bool isAdmin)
    {
        var result = new RowResultObject<WorkerBillingContext>();
        try
        {
            var worker = await RequireWorkerAsync(workerId, personId, isAdmin);
            var businessId = worker.WorkerParentID > 0 ? worker.WorkerParentID : worker.ID;
            var setting = await _context.BusinessBillingSettings.AsNoTracking().SingleOrDefaultAsync(x => x.BusinessID == businessId && x.IsActive);
            var workerSetting = await _context.WorkerBillingSettings.AsNoTracking().SingleOrDefaultAsync(x => x.BusinessID == businessId && x.WorkerID == workerId);
            var subscription = await _context.BusinessSubscriptions.AsNoTracking()
                .Where(x => x.BusinessID == businessId && x.Status == "active" && x.StartsAt <= DateTime.UtcNow && x.EndsAt > DateTime.UtcNow)
                .OrderByDescending(x => x.StartsAt)
                .FirstOrDefaultAsync();
            var planType = subscription?.PlanTypeSnapshot ?? "";
            var registered = subscription == null ? 0 : await GetRegisteredMemberCountAsync(businessId, planType);
            var consumed = subscription == null ? 0 : await GetConsumedMemberCountAsync(businessId, planType);
            var validProfile = subscription != null &&
                ((string.Equals(planType, "Individual", StringComparison.OrdinalIgnoreCase) && worker.ID == businessId && worker.WorkerParentID == 0 && !worker.IsWorkShop && await IsEligibleProviderAsync(worker.ID, worker.ID)) ||
                 (string.Equals(planType, "Business", StringComparison.OrdinalIgnoreCase) && await IsEligibleProviderAsync(worker.ID, businessId)));
            var coverageReason = subscription == null ? "اشتراک فعال برای این پروفایل وجود ندارد" :
                validProfile ? null : "این پروفایل طبق نوع پلن تحت پوشش نیست یا هنوز شرایط ارائه خدمت را ندارد";
            var payer = workerSetting?.FeePayerOverride ?? setting?.DefaultFeePayer ?? "Business";
            result.Result = new WorkerBillingContext
            {
                WorkerID = worker.ID, BusinessID = businessId, IsBookingFeeEnabled = setting?.IsBookingFeeEnabled == true,
                HasActiveSubscription = subscription != null, BookingFeeAmount = subscription?.BookingFeeAmountSnapshot ?? 0,
                ContractVersion = subscription?.ContractVersionSnapshot, FeePayer = payer, CollectionMode = setting?.CollectionMode ?? "Hybrid",
                RequiresWorkerConsent = payer == "Worker" && worker.ID != businessId,
                HasWorkerConsent = workerSetting?.AcceptedByPersonID == worker.PersonID && subscription != null && workerSetting.AcceptedContractVersion == subscription.ContractVersionSnapshot,
                SubscriptionID = subscription?.ID,
                SubscriptionPlanID = subscription?.SubscriptionPlanID,
                PlanType = subscription?.PlanTypeSnapshot,
                TotalCapacity = subscription?.MaxMemberCountSnapshot ?? 0,
                RegisteredMemberCount = registered,
                ConsumedMemberCount = consumed,
                RemainingCapacity = Math.Max(0, (subscription?.MaxMemberCountSnapshot ?? 0) - consumed),
                IsSubscriptionValid = subscription != null,
                IsProfileCovered = validProfile,
                CoverageReason = coverageReason
            };
        }
        catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
        return result;
    }

    public async Task<string?> ValidateWorkerCapacityAsync(long workerId)
    {
        var worker = await _context.Workers.AsNoTracking().SingleOrDefaultAsync(x => x.ID == workerId);
        if (worker == null || worker.IsWorkShop || worker.WorkerParentID <= 0 || !HasApprovedAccount(worker) || !worker.Person.IsActive)
            return null;

        var subscription = await _context.BusinessSubscriptions.AsNoTracking()
            .Where(x => x.BusinessID == worker.WorkerParentID && x.Status == "active" &&
                x.StartsAt <= DateTime.UtcNow && x.EndsAt > DateTime.UtcNow)
            .OrderByDescending(x => x.StartsAt)
            .FirstOrDefaultAsync();
        if (subscription == null || !string.Equals(subscription.PlanTypeSnapshot, "Business", StringComparison.OrdinalIgnoreCase))
            return null;

        if (!await _context.WorkerServices.AnyAsync(x => x.WorkerID == workerId))
            return null;

        var consumed = await GetConsumedMemberCountAsync(worker.WorkerParentID, subscription.PlanTypeSnapshot);
        return consumed > subscription.MaxMemberCountSnapshot
            ? "ظرفیت اشتراک سالن تکمیل است و این متخصص نمی‌تواند به‌عنوان عضو واجد شرایط فعال شود"
            : null;
    }

    private Payment NewPayment(decimal amount, string purpose, long personId) => new()
    {
        Purpose = purpose, OwnerPersonID = personId, CurrencyCode = "IRR", AllPaymentAmount = amount,
        DepositAmount = amount, RemainAmount = amount, PlarformAmount = amount,
        PaymentDate = DateTime.Now, PaymentStatus = "pending", PaymentLevel = 1,
        CreateDate = DateTime.Now, PaymentBookings = new List<PaymentBooking>(), PaymentDetails = new List<PaymentDetail>()
    };

    public Task<BitResultObject> PurchaseSubscriptionAsync(long businessId, long planId, Guid requestId, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        var worker = await RequireWorkerAsync(businessId, personId, isAdmin);
        if (worker.WorkerParentID > 0 && !worker.IsWorkShop) throw new InvalidOperationException("اشتراک از پروفایل کسب‌وکار خریداری می‌شود");
        if (requestId == Guid.Empty) throw new InvalidOperationException("شناسه درخواست الزامی است");
        var old = await _context.BusinessSubscriptions.SingleOrDefaultAsync(x => x.BusinessID == businessId && x.RequestID == requestId);
        if (old != null)
        {
            if (old.SubscriptionPlanID != planId) throw new InvalidOperationException("شناسه درخواست قبلاً با ورودی دیگری استفاده شده است");
            return old.PaymentID;
        }
        var plan = await _context.SubscriptionPlans.SingleOrDefaultAsync(x => x.ID == planId && x.IsActive)
            ?? throw new InvalidOperationException("پلن فعال یافت نشد");
        var isIndependent = !worker.IsWorkShop && worker.WorkerParentID == 0;
        var isBusiness = worker.IsWorkShop && worker.WorkerParentID == 0;
        if (string.Equals(plan.PlanType, "Individual", StringComparison.OrdinalIgnoreCase) && !isIndependent)
            throw new InvalidOperationException("Individual plans require an independent worker profile.");
        if (string.Equals(plan.PlanType, "Business", StringComparison.OrdinalIgnoreCase) && !isBusiness)
            throw new InvalidOperationException("Business plans require a workshop profile.");
        var consumedMembers = await GetConsumedMemberCountAsync(businessId, plan.PlanType);
        if (consumedMembers > plan.MaxMemberCount)
            throw new InvalidOperationException("The selected plan capacity is lower than the current eligible member count.");
        var payment = NewPayment(plan.Price, PaymentPurposes.Subscription, worker.PersonID);
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        _context.BusinessSubscriptions.Add(new BusinessSubscription
        {
            BusinessID = businessId, SubscriptionPlanID = plan.ID, PaymentID = payment.ID, RequestID = requestId,
            PlanNameSnapshot = plan.Name, PlanTypeSnapshot = plan.PlanType, MaxMemberCountSnapshot = plan.MaxMemberCount,
            DurationMonthsSnapshot = plan.DurationMonths, PriceSnapshot = plan.Price,
            BookingFeeAmountSnapshot = plan.BookingFeeAmount, CurrencyCode = plan.CurrencyCode,
            ContractVersionSnapshot = plan.ContractVersion, CreateDate = DateTime.UtcNow
        });
        return payment.ID;
    });

    public Task<BitResultObject> CreateBillingPaymentAsync(long accountId, long? invoiceId, decimal amount, Guid requestId, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        var account = await _context.BillingAccounts.SingleAsync(x => x.ID == accountId && x.IsActive);
        var worker = await RequireWorkerAsync(account.WorkerID, personId, isAdmin);
        if (requestId == Guid.Empty) throw new InvalidOperationException("شناسه درخواست الزامی است");
        invoiceId = invoiceId > 0 ? invoiceId : null;
        var old = await _context.BillingPayments.SingleOrDefaultAsync(x => x.BillingAccountID == accountId && x.RequestID == requestId);
        if (old != null)
        {
            if (old.BillingInvoiceID != invoiceId || (!invoiceId.HasValue && old.Amount != amount))
                throw new InvalidOperationException("شناسه درخواست قبلاً با ورودی دیگری استفاده شده است");
            return old.PaymentID;
        }
        if (invoiceId.HasValue)
        {
            var invoice = await _context.BillingInvoices.SingleAsync(x => x.ID == invoiceId && x.BillingAccountID == accountId);
            await AllocateAsync(accountId, invoice.ID);
            await UpdateInvoicesAsync(accountId);
            amount = await InvoiceRemainingAsync(invoice.ID);
            if (amount == 0) return 0;
        }
        Money(amount);
        var payment = NewPayment(amount, invoiceId.HasValue ? PaymentPurposes.BillingInvoice : PaymentPurposes.BillingCredit, worker.PersonID);
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        _context.BillingPayments.Add(new BillingPayment
        {
            BillingAccountID = accountId, PaymentID = payment.ID, BillingInvoiceID = invoiceId,
            RequestID = requestId, Amount = amount, CreateDate = DateTime.UtcNow
        });
        return payment.ID;
    });

    public async Task<RowResultObject<BillingAccountReport>> GetAccountReportAsync(long workerId, long personId, bool isAdmin)
    {
        var result = new RowResultObject<BillingAccountReport>();
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            await AcquireLockAsync();
            await RequireWorkerAsync(workerId, personId, isAdmin);
            var account = await AccountAsync(workerId);
            var fees = await _context.BookingPlatformFees.AsNoTracking().Where(x => x.BillingAccountID == account.ID).ToListAsync();
            var net = await _context.BillingTransactions.Where(x => x.BillingAccountID == account.ID).SumAsync(x => x.Amount);
            var paid = await _context.BillingPayments.Where(x => x.BillingAccountID == account.ID && x.Status == "confirmed").SumAsync(x => x.Amount);
            var refunds = _context.BillingRefunds.Where(r => _context.BillingPayments.Any(p => p.ID == r.BillingPaymentID && p.BillingAccountID == account.ID));
            var refunded = await refunds.Where(x => x.Status == "confirmed").SumAsync(x => x.Amount);
            var reserved = await refunds.Where(x => x.Status == "pending").SumAsync(x => x.Amount);
            var allocated = await (from a in _context.BillingAllocations join p in _context.BillingPayments on a.BillingPaymentID equals p.ID
                                   where p.BillingAccountID == account.ID && a.ReversedAt == null select a.Amount).SumAsync();
            var total = fees.Sum(x => x.FeeAmount); var reversed = fees.Where(x => x.Status == "reversed").Sum(x => x.FeeAmount);
            var difference = net - (total - reversed - paid + refunded);
            result.Result = new BillingAccountReport
            {
                BillingAccountID = account.ID, WorkerID = workerId, CurrencyCode = account.CurrencyCode, NetBalance = net,
                AmountDue = Math.Max(net, 0), AvailableCredit = Math.Max(-net - reserved, 0), TotalFees = total,
                ReversedFees = reversed, ConfirmedPayments = paid, OpenFees = total - reversed - allocated,
                UnallocatedCredit = paid - allocated - refunded - reserved, ReservedCredit = reserved, RefundedAmount = refunded,
                ReconciliationDifference = difference, IsReconciled = difference == 0
            };
            await transaction.CommitAsync();
        }
        catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
        return result;
    }

    public Task<BitResultObject> ApplyBookingStateAsync(long bookingId) => WriteAsync(async () =>
    {
        var booking = await _context.Bookings.SingleAsync(x => x.ID == bookingId);
        var existing = await _context.BookingPlatformFees.SingleOrDefaultAsync(x => x.BookingID == bookingId);
        if (existing != null)
        {
            if (booking.IsCancelled || booking.Status != "4") await ReverseFeeCoreAsync(existing, "Booking completion reversed");
            return existing.ID;
        }
        if (booking.IsCancelled || booking.Status != "4") return 0;
        var worker = await _context.Workers.AsNoTracking().SingleAsync(x => x.ID == booking.WorkerID);
        var businessId = worker.WorkerParentID > 0 ? worker.WorkerParentID : worker.ID;
        var setting = await _context.BusinessBillingSettings.SingleOrDefaultAsync(x => x.BusinessID == businessId && x.IsActive && x.IsBookingFeeEnabled);
        if (setting == null) return 0;
        var now = DateTime.UtcNow;
        var subscription = await _context.BusinessSubscriptions.Where(x => x.BusinessID == businessId && x.Status == "active" && x.StartsAt <= now && x.EndsAt > now)
            .OrderByDescending(x => x.StartsAt).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("اشتراک معتبر برای ثبت کارمزد نوبت وجود ندارد");
        var workerSetting = await _context.WorkerBillingSettings.SingleOrDefaultAsync(x => x.BusinessID == businessId && x.WorkerID == worker.ID);
        var payer = workerSetting?.FeePayerOverride ?? setting.DefaultFeePayer;
        if (payer == "Worker" && worker.ID != businessId &&
            (workerSetting?.AcceptedByPersonID != worker.PersonID || workerSetting.AcceptedContractVersion != subscription.ContractVersionSnapshot))
            throw new InvalidOperationException("متخصص هنوز پرداخت کارمزد را نپذیرفته است");
        var account = await AccountAsync(payer == "Worker" ? worker.ID : businessId);
        if (!account.IsActive) throw new InvalidOperationException("حساب مالی غیرفعال است");
        var net = await _context.BillingTransactions.Where(x => x.BillingAccountID == account.ID).SumAsync(x => x.Amount);
        var reserved = await _context.BillingRefunds.Where(r => r.Status == "pending" && _context.BillingPayments.Any(p => p.ID == r.BillingPaymentID && p.BillingAccountID == account.ID)).SumAsync(x => x.Amount);
        var feeAmount = subscription.BookingFeeAmountSnapshot;
        if (setting.CollectionMode == "Prepaid" && net + reserved + feeAmount > 0)
            throw new InvalidOperationException("اعتبار کافی برای کارمزد نوبت وجود ندارد؛ ابتدا حساب مالی را شارژ کنید");
        if (setting.CollectionMode != "Prepaid" && setting.CreditLimit > 0 && net + reserved + feeAmount > setting.CreditLimit)
            throw new InvalidOperationException("سقف بدهی حساب مالی تکمیل شده است");
        var fee = new BookingPlatformFee
        {
            BookingID = booking.ID, BusinessID = businessId, WorkerID = worker.ID, BillingAccountID = account.ID,
            BusinessSubscriptionID = subscription.ID, FeeAmount = feeAmount, CurrencyCode = subscription.CurrencyCode,
            FeePayerSnapshot = payer, CollectionModeSnapshot = setting.CollectionMode,
            ContractVersionSnapshot = subscription.ContractVersionSnapshot, AccruedAt = now, CreateDate = now
        };
        _context.BookingPlatformFees.Add(fee);
        await _context.SaveChangesAsync();
        if (feeAmount > 0) Ledger(account.ID, feeAmount, "fee", "BookingPlatformFee", fee.ID);
        await _context.SaveChangesAsync();
        if (setting.CollectionMode != "Postpaid") await AllocateAsync(account.ID);
        return fee.ID;
    });

    private void Ledger(long accountId, decimal amount, string type, string source, long id) =>
        _context.BillingTransactions.Add(new BillingTransaction
        {
            BillingAccountID = accountId, Amount = amount, TransactionType = type, SourceType = source,
            SourceID = id, OccurredAt = DateTime.UtcNow, CreateDate = DateTime.UtcNow
        });

    public Task<BitResultObject> ReverseFeeAsync(long feeId, string reason, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        if (!isAdmin || string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("اصلاح مالی فقط توسط مدیر و با ذکر دلیل مجاز است");
        var fee = await _context.BookingPlatformFees.SingleAsync(x => x.ID == feeId);
        await ReverseFeeCoreAsync(fee, $"Person:{personId}; {reason}");
        return fee.ID;
    });

    private async Task ReverseFeeCoreAsync(BookingPlatformFee fee, string reason)
    {
        if (fee.Status == "reversed") return;
        fee.Status = "reversed"; fee.ReversedAt = DateTime.UtcNow; fee.Description = reason; fee.UpdateDate = DateTime.UtcNow;
        if (fee.FeeAmount > 0) Ledger(fee.BillingAccountID, -fee.FeeAmount, "reversal", "BookingPlatformFee", fee.ID);
        var allocations = await _context.BillingAllocations.Where(x => x.BookingPlatformFeeID == fee.ID && x.ReversedAt == null).ToListAsync();
        foreach (var allocation in allocations) allocation.ReversedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await AllocateAsync(fee.BillingAccountID);
        await UpdateInvoicesAsync(fee.BillingAccountID);
    }

    private async Task AllocateAsync(long accountId, long? invoiceId = null)
    {
        var payments = await _context.BillingPayments.Where(x => x.BillingAccountID == accountId && x.Status == "confirmed")
            .OrderBy(x => x.ID).ToListAsync();
        var feesQuery = _context.BookingPlatformFees.Where(x => x.BillingAccountID == accountId && x.Status == "accrued" && x.FeeAmount > 0);
        if (invoiceId.HasValue)
            feesQuery = feesQuery.Where(f => _context.BillingInvoiceItems.Any(i => i.BillingInvoiceID == invoiceId && i.BookingPlatformFeeID == f.ID));
        else
            feesQuery = feesQuery.Where(f => f.CollectionModeSnapshot != "Postpaid");
        var fees = await feesQuery.OrderBy(x => x.AccruedAt).ThenBy(x => x.ID).ToListAsync();
        var allocations = await (from a in _context.BillingAllocations join p in _context.BillingPayments on a.BillingPaymentID equals p.ID
                                  where p.BillingAccountID == accountId && a.ReversedAt == null select a).ToListAsync();
        foreach (var payment in payments)
        {
            var available = payment.Amount - allocations.Where(x => x.BillingPaymentID == payment.ID).Sum(x => x.Amount);
            available -= await _context.BillingRefunds.Where(x => x.BillingPaymentID == payment.ID && x.Status != "rejected").SumAsync(x => x.Amount);
            foreach (var fee in fees)
            {
                if (available <= 0) break;
                var remaining = fee.FeeAmount - allocations.Where(x => x.BookingPlatformFeeID == fee.ID).Sum(x => x.Amount);
                var amount = Math.Min(available, remaining);
                if (amount <= 0) continue;
                var allocation = allocations.SingleOrDefault(x => x.BillingPaymentID == payment.ID && x.BookingPlatformFeeID == fee.ID);
                if (allocation == null)
                {
                    allocation = new BillingAllocation { BillingPaymentID = payment.ID, BookingPlatformFeeID = fee.ID, AllocatedAt = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
                    allocations.Add(allocation); _context.BillingAllocations.Add(allocation);
                }
                allocation.Amount += amount; available -= amount;
            }
        }
        await _context.SaveChangesAsync();
    }

    private async Task<decimal> InvoiceRemainingAsync(long invoiceId)
    {
        var feeIds = _context.BillingInvoiceItems.Where(x => x.BillingInvoiceID == invoiceId).Select(x => x.BookingPlatformFeeID);
        var total = await _context.BookingPlatformFees.Where(x => feeIds.Contains(x.ID) && x.Status == "accrued").SumAsync(x => x.FeeAmount);
        var allocated = await _context.BillingAllocations.Where(x => feeIds.Contains(x.BookingPlatformFeeID) && x.ReversedAt == null).SumAsync(x => x.Amount);
        return Math.Max(0, total - allocated);
    }

    private async Task UpdateInvoicesAsync(long accountId)
    {
        var invoices = await _context.BillingInvoices.Where(x => x.BillingAccountID == accountId).ToListAsync();
        foreach (var invoice in invoices) invoice.Status = await InvoiceRemainingAsync(invoice.ID) == 0 ? "paid" : invoice.DueDate < DateTime.UtcNow ? "overdue" : "open";
        await _context.SaveChangesAsync();
    }

    public Task<BitResultObject> IssueInvoiceAsync(long accountId, DateTime start, DateTime end, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        var account = await _context.BillingAccounts.SingleAsync(x => x.ID == accountId);
        await RequireWorkerAsync(account.WorkerID, personId, isAdmin);
        return await IssueInvoiceCoreAsync(account, start, end);
    });

    private async Task<long> IssueInvoiceCoreAsync(BillingAccount account, DateTime start, DateTime end)
    {
        if (end <= start || end > DateTime.UtcNow || start.Year < 2000) throw new InvalidOperationException("بازه صورتحساب معتبر نیست یا هنوز پایان نیافته است");
        var existing = await _context.BillingInvoices.SingleOrDefaultAsync(x => x.BillingAccountID == account.ID && x.PeriodStart == start && x.PeriodEnd == end);
        if (existing != null) return existing.ID;
        var fees = await _context.BookingPlatformFees.Where(x => x.BillingAccountID == account.ID && x.Status == "accrued" && x.AccruedAt >= start && x.AccruedAt < end
            && !_context.BillingInvoiceItems.Any(i => i.BookingPlatformFeeID == x.ID)).ToListAsync();
        var items = new List<BillingInvoiceItem>();
        foreach (var fee in fees)
        {
            var allocated = await _context.BillingAllocations.Where(x => x.BookingPlatformFeeID == fee.ID && x.ReversedAt == null).SumAsync(x => x.Amount);
            if (fee.FeeAmount > allocated) items.Add(new BillingInvoiceItem { BookingPlatformFeeID = fee.ID, Amount = fee.FeeAmount - allocated, CreateDate = DateTime.UtcNow });
        }
        if (items.Count == 0) return 0;
        var worker = await _context.Workers.AsNoTracking().SingleAsync(x => x.ID == account.WorkerID);
        var businessId = worker.WorkerParentID > 0 ? worker.WorkerParentID : worker.ID;
        var dueDays = await _context.BusinessBillingSettings.Where(x => x.BusinessID == businessId).Select(x => (int?)x.DueDays).FirstOrDefaultAsync() ?? 7;
        var invoice = new BillingInvoice { BillingAccountID = account.ID, PeriodStart = start, PeriodEnd = end, IssueDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(dueDays), TotalAmount = items.Sum(x => x.Amount), CurrencyCode = account.CurrencyCode, CreateDate = DateTime.UtcNow };
        _context.BillingInvoices.Add(invoice);
        await _context.SaveChangesAsync();
        foreach (var item in items) { item.BillingInvoiceID = invoice.ID; _context.BillingInvoiceItems.Add(item); }
        return invoice.ID;
    }

    public Task<BitResultObject> ConfirmGatewayPaymentAsync(long historyId, string transactionCode, string trackingNumber, string? token, string? message) => WriteAsync(async () =>
    {
        var history = await _context.PaymentHistories.SingleAsync(x => x.ID == historyId);
        if (history.PaymentStatus) return history.ID;
        if (string.IsNullOrWhiteSpace(transactionCode)) throw new InvalidOperationException("شناسه تراکنش درگاه دریافت نشد");
        var payment = await _context.Payments.SingleAsync(x => x.ID == history.PaymentID);
        if (payment.PaymentFinished || history.Amount <= 0 || history.Amount > payment.AllPaymentAmount - payment.PayedAmount)
            throw new InvalidOperationException("مبلغ تاییدشده با مانده پرداخت سازگار نیست؛ بررسی مالی لازم است");
        payment.PayedAmount += history.Amount;
        payment.RemainAmount = Math.Max(0, payment.AllPaymentAmount - payment.PayedAmount);
        payment.PaymentFinished = payment.RemainAmount == 0;
        payment.PaymentStatus = payment.PaymentFinished ? "paid" : "partially_paid";
        payment.UpdateDate = DateTime.Now; payment.PaymentDate = DateTime.Now;
        history.PaymentStatus = true; history.TransactionCode = transactionCode; history.TrackingNumber = trackingNumber;
        history.GatewayRequestState = "verified";
        history.PaymentToken = token; history.GatewayMessage = message; history.PaymentDate = DateTime.Now; history.UpdateDate = DateTime.Now;
        if (payment.Purpose == PaymentPurposes.Subscription)
        {
            if (!payment.PaymentFinished) throw new InvalidOperationException("اشتراک باید به طور کامل پرداخت شود");
            var subscription = await _context.BusinessSubscriptions.SingleAsync(x => x.PaymentID == payment.ID);
            var latestEnd = await _context.BusinessSubscriptions.Where(x => x.BusinessID == subscription.BusinessID && x.Status == "active")
                .MaxAsync(x => x.EndsAt);
            var start = latestEnd > DateTime.UtcNow ? latestEnd.Value : DateTime.UtcNow;
            subscription.StartsAt = start; subscription.EndsAt = start.AddMonths(subscription.DurationMonthsSnapshot);
            subscription.Status = "active"; subscription.UpdateDate = DateTime.UtcNow;
        }
        else if (payment.Purpose == PaymentPurposes.BillingCredit || payment.Purpose == PaymentPurposes.BillingInvoice)
        {
            if (!payment.PaymentFinished) throw new InvalidOperationException("پرداخت حساب مالی باید کامل باشد");
            var billing = await _context.BillingPayments.SingleAsync(x => x.PaymentID == payment.ID);
            billing.Status = "confirmed"; billing.ConfirmedAt = DateTime.UtcNow;
            Ledger(billing.BillingAccountID, -billing.Amount, "payment", "BillingPayment", billing.ID);
            await _context.SaveChangesAsync();
            if (billing.BillingInvoiceID.HasValue) await AllocateAsync(billing.BillingAccountID, billing.BillingInvoiceID);
            await AllocateAsync(billing.BillingAccountID);
            await UpdateInvoicesAsync(billing.BillingAccountID);

        }
        else if (payment.Purpose != PaymentPurposes.Service) throw new InvalidOperationException("نوع پرداخت معتبر نیست");
        return history.ID;
    });

    public Task<BitResultObject> ProcessBillingAsync() => WriteAsync(async () =>
    {
        var now = DateTime.UtcNow;
        var expired = await _context.BusinessSubscriptions.Where(x => x.Status == "active" && x.EndsAt <= now).ToListAsync();
        foreach (var subscription in expired) subscription.Status = "expired";
        var settings = await _context.BusinessBillingSettings.Where(x => x.IsActive && x.IsBookingFeeEnabled).ToListAsync();
        foreach (var setting in settings)
        {
            var ownerIds = await _context.Workers.Where(x => x.ID == setting.BusinessID || x.WorkerParentID == setting.BusinessID).Select(x => x.ID).ToListAsync();
            var accounts = await _context.BillingAccounts.Where(x => ownerIds.Contains(x.WorkerID) && x.IsActive).ToListAsync();
            var end = setting.InvoiceCycle == "Monthly" ? new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                : now.Date.AddDays(-(((int)now.DayOfWeek + 6) % 7));
            foreach (var account in accounts)
            {
                await AllocateAsync(account.ID);
                // Include unbilled arrears from missed job runs, without regenerating debt.
                var firstFee = await _context.BookingPlatformFees.Where(x => x.BillingAccountID == account.ID && x.Status == "accrued" && x.AccruedAt < end
                    && !_context.BillingInvoiceItems.Any(i => i.BookingPlatformFeeID == x.ID)).MinAsync(x => (DateTime?)x.AccruedAt);
                if (firstFee.HasValue) await IssueInvoiceCoreAsync(account, firstFee.Value, end);
                await UpdateInvoicesAsync(account.ID);
            }
        }
        return settings.Count;
    });

    public Task<BitResultObject> RequestRefundAsync(long billingPaymentId, decimal amount, Guid requestId, string reason, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        Money(amount);
        if (requestId == Guid.Empty || string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("شناسه درخواست و دلیل بازپرداخت الزامی است");
        var payment = await _context.BillingPayments.SingleAsync(x => x.ID == billingPaymentId && x.Status == "confirmed");
        var account = await _context.BillingAccounts.SingleAsync(x => x.ID == payment.BillingAccountID);
        await RequireWorkerAsync(account.WorkerID, personId, isAdmin);
        var old = await _context.BillingRefunds.SingleOrDefaultAsync(x => x.BillingPaymentID == payment.ID && x.RequestID == requestId);
        if (old != null)
        {
            if (old.Amount != amount) throw new InvalidOperationException("شناسه درخواست با مبلغ دیگری استفاده شده است");
            return old.ID;
        }
        var allocated = await _context.BillingAllocations.Where(x => x.BillingPaymentID == payment.ID && x.ReversedAt == null).SumAsync(x => x.Amount);
        var refunds = await _context.BillingRefunds.Where(x => x.BillingPaymentID == payment.ID && x.Status != "rejected").SumAsync(x => x.Amount);
        var net = await _context.BillingTransactions.Where(x => x.BillingAccountID == account.ID).SumAsync(x => x.Amount);
        var reserved = await _context.BillingRefunds.Where(r => r.Status == "pending" && _context.BillingPayments.Any(p => p.ID == r.BillingPaymentID && p.BillingAccountID == account.ID)).SumAsync(x => x.Amount);
        if (amount > payment.Amount - allocated - refunds || amount > Math.Max(0, -net - reserved))
            throw new InvalidOperationException("مبلغ بازپرداخت از اعتبار آزاد و تخصیص‌نیافته بیشتر است");
        var row = new BillingRefund { BillingPaymentID = payment.ID, RequestID = requestId, Amount = amount, RequestedByPersonID = personId, Description = reason, CreateDate = DateTime.UtcNow };
        _context.BillingRefunds.Add(row); await _context.SaveChangesAsync();
        return row.ID;
    });

    public Task<BitResultObject> ResolveRefundAsync(long refundId, bool confirm, string? reference, string reason, long personId, bool isAdmin) => WriteAsync(async () =>
    {
        if (!isAdmin) throw new InvalidOperationException("تایید بازپرداخت مخصوص مدیر است");
        var row = await _context.BillingRefunds.SingleAsync(x => x.ID == refundId);
        if (row.Status != "pending")
        {
            if ((row.Status == "confirmed") == confirm) return row.ID;
            throw new InvalidOperationException("این درخواست قبلاً تعیین تکلیف شده است");
        }
        if (confirm && (string.IsNullOrWhiteSpace(reference) || reference.Length > 200))
            throw new InvalidOperationException("ثبت کد بانکی بازپرداخت واقعی الزامی است");
        if (!confirm && string.IsNullOrWhiteSpace(reason)) throw new InvalidOperationException("دلیل رد الزامی است");
        var payment = await _context.BillingPayments.SingleAsync(x => x.ID == row.BillingPaymentID);
        row.Status = confirm ? "confirmed" : "rejected"; row.ResolvedAt = DateTime.UtcNow;
        row.ResolvedByPersonID = personId; row.BankReference = confirm ? reference!.Trim() : null;
        row.Description += "\nResolution: " + reason;
        if (confirm) Ledger(payment.BillingAccountID, row.Amount, "refund", "BillingRefund", row.ID);
        await _context.SaveChangesAsync();
        if (!confirm) await AllocateAsync(payment.BillingAccountID);
        await UpdateInvoicesAsync(payment.BillingAccountID);
        return row.ID;
    });

    public async Task<ListResultObject<BillingReconciliationIssue>> GetReconciliationReportAsync(bool isAdmin)
    {
        var result = new ListResultObject<BillingReconciliationIssue>();
        try
        {
            if (!isAdmin) throw new InvalidOperationException("گزارش مغایرت مخصوص مدیر است");
            var missing = await (from g in _context.GatewayPaymentRecords.AsNoTracking()
                                 join h in _context.PaymentHistories on g.TrackingNumber equals h.ID
                                 where g.IsPaid && !h.PaymentStatus
                                 select new BillingReconciliationIssue { IssueType = "gateway_verified_not_posted", PaymentID = h.PaymentID, PaymentHistoryID = h.ID, ExpectedAmount = g.Amount, ActualAmount = 0 }).ToListAsync();
            var wrongTotals = await _context.Payments.AsNoTracking().Where(p => p.OwnerPersonID != null &&
                p.PayedAmount != _context.PaymentHistories.Where(h => h.PaymentID == p.ID && h.PaymentStatus).Sum(h => h.Amount))
                .Select(p => new BillingReconciliationIssue { IssueType = "payment_total_mismatch", PaymentID = p.ID, ExpectedAmount = _context.PaymentHistories.Where(h => h.PaymentID == p.ID && h.PaymentStatus).Sum(h => h.Amount), ActualAmount = p.PayedAmount }).ToListAsync();
            var missingLedger = await _context.BillingPayments.AsNoTracking().Where(p => p.Status == "confirmed" &&
                !_context.BillingTransactions.Any(t => t.SourceType == "BillingPayment" && t.SourceID == p.ID && t.TransactionType == "payment"))
                .Select(p => new BillingReconciliationIssue { IssueType = "missing_billing_payment_ledger", PaymentID = p.PaymentID, BillingAccountID = p.BillingAccountID, ExpectedAmount = -p.Amount, ActualAmount = 0 }).ToListAsync();
            var overAllocated = await _context.BillingPayments.AsNoTracking().Where(p =>
                _context.BillingAllocations.Where(a => a.BillingPaymentID == p.ID && a.ReversedAt == null).Sum(a => a.Amount) > p.Amount)
                .Select(p => new BillingReconciliationIssue { IssueType = "payment_over_allocated", PaymentID = p.PaymentID, BillingAccountID = p.BillingAccountID, ExpectedAmount = p.Amount, ActualAmount = _context.BillingAllocations.Where(a => a.BillingPaymentID == p.ID && a.ReversedAt == null).Sum(a => a.Amount) }).ToListAsync();
            result.Results = missing.Concat(wrongTotals).Concat(missingLedger).Concat(overAllocated).ToList();
            result.TotalCount = result.Results.Count; result.PageCount = result.TotalCount == 0 ? 0 : 1;
        }
        catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
        return result;
    }
}
