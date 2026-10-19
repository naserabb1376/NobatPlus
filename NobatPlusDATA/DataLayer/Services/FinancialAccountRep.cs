using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class FinancialAccountRep : IFinancialAccountRep
    {
        private readonly NobatPlusContext _context;

        public FinancialAccountRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<RowResultObject<FinancialAccountReport>> GetFinancialAccountAsync(long workerId, int pageIndex = 1, int pageSize = 20, DateTime? fromDate = null, DateTime? toDate = null, string transactionType = "")
        {
            var result = new RowResultObject<FinancialAccountReport>();
            try
            {
                var account = await GetOrCreateAccountAsync(workerId);
                await SyncEarningsAsync(account);

                var pendingSettlementAmount = await _context.SettlementRequests
                    .Where(x => x.FinancialAccountID == account.ID && x.Status == "pending")
                    .SumAsync(x => x.Amount);

                var transactionQuery = _context.FinancialTransactions
                    .AsNoTracking()
                    .Where(x => x.FinancialAccountID == account.ID);

                if (fromDate.HasValue)
                    transactionQuery = transactionQuery.Where(x => x.TransactionDate >= fromDate.Value);
                if (toDate.HasValue)
                {
                    var inclusiveEnd = toDate.Value.Date.AddDays(1);
                    transactionQuery = transactionQuery.Where(x => x.TransactionDate < inclusiveEnd);
                }
                if (!string.IsNullOrWhiteSpace(transactionType))
                    transactionQuery = transactionQuery.Where(x => x.TransactionType == transactionType);

                var filteredRows = await transactionQuery.ToListAsync();
                var transactions = await transactionQuery
                    .OrderByDescending(x => x.TransactionDate)
                    .ThenByDescending(x => x.ID)
                    .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
                    .Take(Math.Max(pageSize, 1))
                    .Select(x => new FinancialTransactionReport
                    {
                        Id = x.ID,
                        Amount = x.Amount,
                        TransactionType = x.TransactionType,
                        Status = x.Status,
                        TransactionDate = x.TransactionDate,
                        BookingId = x.BookingID,
                        PaymentId = x.PaymentID,
                        Description = x.Description ?? "",
                        ReferenceNumber = x.ReferenceNumber
                    })
                    .ToListAsync();

                var dailyReport = filteredRows
                    .GroupBy(x => x.TransactionDate.Date)
                    .OrderBy(x => x.Key)
                    .Select(group => new FinancialDailyReport
                    {
                        Date = group.Key,
                        Income = group.Where(x => x.TransactionType == "earning").Sum(x => x.Amount),
                        Outcome = group.Where(x =>
                            x.TransactionType == "settlement_paid" ||
                            x.TransactionType == "cancellation_reversal").Sum(x => x.Amount),
                        Count = group.Count()
                    })
                    .ToList();

                var settlements = await _context.SettlementRequests
                    .AsNoTracking()
                    .Where(x => x.FinancialAccountID == account.ID)
                    .OrderByDescending(x => x.RequestDate)
                    .ThenByDescending(x => x.ID)
                    .Take(20)
                    .Select(x => new SettlementRequestReport
                    {
                        Id = x.ID,
                        Amount = x.Amount,
                        Status = x.Status,
                        RequestDate = x.RequestDate,
                        SettlementDate = x.SettlementDate,
                        TrackingCode = x.TrackingCode,
                        Description = x.Description ?? ""
                    })
                    .ToListAsync();

                var grossEarned = await _context.FinancialTransactions
                    .Where(x => x.FinancialAccountID == account.ID && x.TransactionType == "earning" && x.Status == "success")
                    .SumAsync(x => x.Amount);
                var cancelledEarnings = await _context.FinancialTransactions
                    .Where(x => x.FinancialAccountID == account.ID && x.TransactionType == "cancellation_reversal" && x.Status == "success")
                    .SumAsync(x => x.Amount);
                var totalEarned = grossEarned - cancelledEarnings;

                var totalSettled = await _context.SettlementRequests
                    .Where(x => x.FinancialAccountID == account.ID && x.Status == "paid")
                    .SumAsync(x => x.Amount);

                result.Result = new FinancialAccountReport
                {
                    AccountId = account.ID,
                    WorkerId = account.WorkerID,
                    AccountType = account.AccountType,
                    Balance = account.Balance,
                    PendingSettlementAmount = pendingSettlementAmount,
                    AvailableBalance = Math.Max(0, account.Balance - pendingSettlementAmount),
                    TotalEarned = totalEarned,
                    TotalSettled = totalSettled,
                    FilteredIncome = filteredRows.Where(x => x.TransactionType == "earning").Sum(x => x.Amount)
                        - filteredRows.Where(x => x.TransactionType == "cancellation_reversal").Sum(x => x.Amount),
                    FilteredOutcome = filteredRows.Where(x =>
                        x.TransactionType == "settlement_paid" ||
                        x.TransactionType == "cancellation_reversal").Sum(x => x.Amount),
                    AverageTransactionAmount = filteredRows.Count > 0 ? filteredRows.Average(x => x.Amount) : 0,
                    FilteredTransactionCount = filteredRows.Count,
                    Iban = account.Iban,
                    BankAccountOwnerName = account.BankAccountOwnerName,
                    Transactions = transactions,
                    SettlementRequests = settlements,
                    DailyReport = dailyReport
                };
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> UpdateBankInfoAsync(long workerId, string iban, string ownerName)
        {
            var result = new BitResultObject();
            try
            {
                var account = await GetOrCreateAccountAsync(workerId);
                account.Iban = iban ?? "";
                account.BankAccountOwnerName = ownerName ?? "";
                account.UpdateDate = DateTime.Now.ToShamsi();
                _context.FinancialAccounts.Update(account);
                await _context.SaveChangesAsync();
                result.ID = account.ID;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> RequestSettlementAsync(long workerId, decimal amount, string description = "")
        {
            var result = new BitResultObject();
            try
            {
                if (amount <= 0)
                {
                    result.Status = false;
                    result.ErrorMessage = "مبلغ تسویه معتبر نیست";
                    return result;
                }

                var accountReport = await GetFinancialAccountAsync(workerId);
                if (!accountReport.Status || accountReport.Result == null)
                {
                    result.Status = false;
                    result.ErrorMessage = accountReport.ErrorMessage;
                    return result;
                }

                if (amount > accountReport.Result.AvailableBalance)
                {
                    result.Status = false;
                    result.ErrorMessage = "موجودی قابل تسویه کافی نیست";
                    return result;
                }

                var now = DateTime.Now.ToShamsi();
                var request = new SettlementRequest
                {
                    FinancialAccountID = accountReport.Result.AccountId,
                    Amount = amount,
                    Status = "pending",
                    RequestDate = now,
                    Iban = accountReport.Result.Iban,
                    BankAccountOwnerName = accountReport.Result.BankAccountOwnerName,
                    TrackingCode = "",
                    RejectReason = "",
                    Description = description,
                    CreateDate = now,
                    UpdateDate = now
                };

                await _context.SettlementRequests.AddAsync(request);
                await _context.SaveChangesAsync();

                await _context.FinancialTransactions.AddAsync(new FinancialTransaction
                {
                    FinancialAccountID = accountReport.Result.AccountId,
                    SettlementRequestID = request.ID,
                    Amount = amount,
                    TransactionType = "settlement_request",
                    Status = "pending",
                    TransactionDate = now,
                    ReferenceNumber = Guid.NewGuid().ToString("N"),
                    Description = description,
                    CreateDate = now,
                    UpdateDate = now
                });
                await _context.SaveChangesAsync();

                result.ID = request.ID;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<ListResultObject<AdminSettlementRequestReport>> GetSettlementRequestsAsync(string status = "", int pageIndex = 1, int pageSize = 20, string searchText = "", string accountType = "", DateTime? fromDate = null, DateTime? toDate = null, string sortQuery = "")
        {
            var result = new ListResultObject<AdminSettlementRequestReport>();
            try
            {
                var query = _context.SettlementRequests
                    .Include(x => x.FinancialAccount).ThenInclude(x => x.Worker).ThenInclude(x => x.Person)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(x => x.Status == status);
                }

                if (!string.IsNullOrWhiteSpace(accountType))
                {
                    query = query.Where(x => x.FinancialAccount.AccountType == accountType);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.RequestDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    var inclusiveEnd = toDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(x => x.RequestDate <= inclusiveEnd);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.FinancialAccount.Worker.WorkerName.Contains(searchText) ||
                        x.FinancialAccount.Worker.Person.FirstName.Contains(searchText) ||
                        x.FinancialAccount.Worker.Person.LastName.Contains(searchText) ||
                        x.FinancialAccount.Worker.Person.PhoneNumber.Contains(searchText) ||
                        x.Iban.Contains(searchText) ||
                        x.BankAccountOwnerName.Contains(searchText) ||
                        x.TrackingCode.Contains(searchText) ||
                        x.RejectReason.Contains(searchText) ||
                        x.Description.Contains(searchText) ||
                        x.Amount.ToString().Contains(searchText));
                }

                result.TotalCount = await query.CountAsync();
                result.PageCount = DbTools.GetPageCount(result.TotalCount, pageSize);
                result.Results = await query
                    .OrderByDescending(x => x.RequestDate)
                    .SortBy(sortQuery)
                    .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
                    .Take(Math.Max(pageSize, 1))
                    .Select(x => new AdminSettlementRequestReport
                    {
                        Id = x.ID,
                        FinancialAccountId = x.FinancialAccountID,
                        WorkerId = x.FinancialAccount.WorkerID,
                        AccountType = x.FinancialAccount.AccountType,
                        WorkerName = x.FinancialAccount.Worker.WorkerName,
                        PersonFullName = x.FinancialAccount.Worker.Person.FirstName + " " + x.FinancialAccount.Worker.Person.LastName,
                        Amount = x.Amount,
                        Status = x.Status,
                        RequestDate = x.RequestDate,
                        SettlementDate = x.SettlementDate,
                        TrackingCode = x.TrackingCode,
                        Iban = x.Iban,
                        BankAccountOwnerName = x.BankAccountOwnerName,
                        RejectReason = x.RejectReason,
                        Description = x.Description ?? ""
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> UpdateSettlementStatusAsync(long settlementRequestId, string status, string trackingCode = "", string rejectReason = "")
        {
            var result = new BitResultObject();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var request = await _context.SettlementRequests
                    .Include(x => x.FinancialAccount)
                    .FirstOrDefaultAsync(x => x.ID == settlementRequestId);

                if (request == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "درخواست تسویه یافت نشد";
                    return result;
                }

                if (request.Status == "paid" || request.Status == "rejected")
                {
                    result.Status = false;
                    result.ErrorMessage = "وضعیت این درخواست قبلاً نهایی شده است";
                    return result;
                }

                var normalizedStatus = (status ?? "").Trim().ToLower();
                if (normalizedStatus != "paid" && normalizedStatus != "rejected")
                {
                    result.Status = false;
                    result.ErrorMessage = "وضعیت تسویه معتبر نیست";
                    return result;
                }

                var now = DateTime.Now.ToShamsi();
                request.Status = normalizedStatus;
                request.TrackingCode = trackingCode ?? "";
                request.RejectReason = rejectReason ?? "";
                request.SettlementDate = normalizedStatus == "paid" ? now : null;
                request.UpdateDate = now;

                if (normalizedStatus == "paid")
                {
                    if (request.FinancialAccount.Balance < request.Amount)
                    {
                        result.Status = false;
                        result.ErrorMessage = "موجودی حساب مالی برای پرداخت این تسویه کافی نیست";
                        return result;
                    }

                    var settlementPaidExists = await _context.FinancialTransactions
                        .AnyAsync(x => x.SettlementRequestID == request.ID && x.TransactionType == "settlement_paid");

                    if (settlementPaidExists)
                    {
                        result.Status = false;
                        result.ErrorMessage = "برای این درخواست قبلاً تراکنش پرداخت تسویه ثبت شده است";
                        return result;
                    }

                    request.FinancialAccount.Balance -= request.Amount;
                    request.FinancialAccount.UpdateDate = now;
                    _context.FinancialAccounts.Update(request.FinancialAccount);

                    await _context.FinancialTransactions.AddAsync(new FinancialTransaction
                    {
                        FinancialAccountID = request.FinancialAccountID,
                        SettlementRequestID = request.ID,
                        Amount = request.Amount,
                        TransactionType = "settlement_paid",
                        Status = "paid",
                        TransactionDate = now,
                        ReferenceNumber = string.IsNullOrWhiteSpace(trackingCode) ? Guid.NewGuid().ToString("N") : trackingCode,
                        Description = "پرداخت تسویه",
                        CreateDate = now,
                        UpdateDate = now
                    });
                }

                _context.SettlementRequests.Update(request);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.ID = request.ID;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private async Task<FinancialAccount> GetOrCreateAccountAsync(long workerId)
        {
            var account = await _context.FinancialAccounts.FirstOrDefaultAsync(x => x.WorkerID == workerId);
            if (account != null)
            {
                return account;
            }

            var worker = await _context.Workers.AsNoTracking().FirstOrDefaultAsync(x => x.ID == workerId);
            account = new FinancialAccount
            {
                WorkerID = workerId,
                AccountType = worker?.IsWorkShop == true ? "workshop" : "worker",
                Balance = 0,
                Iban = "",
                BankAccountOwnerName = "",
                IsActive = true,
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi()
            };

            await _context.FinancialAccounts.AddAsync(account);
            await _context.SaveChangesAsync();
            return account;
        }

        private async Task SyncEarningsAsync(FinancialAccount account)
        {
            var accountWorker = await _context.Workers.AsNoTracking().FirstOrDefaultAsync(x => x.ID == account.WorkerID);
            if (accountWorker == null)
            {
                return;
            }

            var workerIds = account.AccountType == "workshop"
                ? await _context.Workers
                    .AsNoTracking()
                    .Where(x => x.ID == account.WorkerID || x.WorkerParentID == account.WorkerID)
                    .Select(x => x.ID)
                    .ToListAsync()
                : new List<long> { account.WorkerID };

            var paidPayments = await _context.Payments
                .Include(x => x.PaymentBookings).ThenInclude(x => x.Booking)
                .AsNoTracking()
                .Where(x => x.PaymentFinished || x.PayedAmount > 0)
                .Where(x => x.PaymentBookings.Any(pb =>
                    workerIds.Contains(pb.Booking.WorkerID) && !pb.Booking.IsCancelled))
                .Select(x => new
                {
                    x.ID,
                    BookingID = x.PaymentBookings
                        .Where(pb => workerIds.Contains(pb.Booking.WorkerID))
                        .Select(pb => pb.BookingID)
                        .FirstOrDefault(),
                    x.WorkerAmount,
                    x.PayedAmount,
                    x.PaymentDate
                })
                .ToListAsync();

            var existingPaymentIds = await _context.FinancialTransactions
                .AsNoTracking()
                .Where(x => x.FinancialAccountID == account.ID && x.TransactionType == "earning" && x.PaymentID.HasValue)
                .Select(x => x.PaymentID!.Value)
                .ToListAsync();

            var existingSet = existingPaymentIds.ToHashSet();
            var now = DateTime.Now.ToShamsi();
            var newTransactions = paidPayments
                .Where(x => !existingSet.Contains(x.ID))
                .Select(x => new FinancialTransaction
                {
                    FinancialAccountID = account.ID,
                    BookingID = x.BookingID,
                    PaymentID = x.ID,
                    Amount = x.WorkerAmount,
                    TransactionType = "earning",
                    Status = "success",
                    TransactionDate = x.PaymentDate == default ? now : x.PaymentDate,
                    ReferenceNumber = Guid.NewGuid().ToString("N"),
                    Description = "درآمد رزرو",
                    CreateDate = now,
                    UpdateDate = now
                })
                .Where(x => x.Amount > 0)
                .ToList();

            if (!newTransactions.Any())
            {
                return;
            }

            await _context.FinancialTransactions.AddRangeAsync(newTransactions);
            account.Balance += newTransactions.Sum(x => x.Amount);
            account.UpdateDate = now;
            _context.FinancialAccounts.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
