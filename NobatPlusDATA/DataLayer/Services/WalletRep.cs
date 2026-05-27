using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class WalletRep : IWalletRep
    {
        private readonly NobatPlusContext _context;
        private readonly IPaymentRep _paymentRep;

        public WalletRep(NobatPlusContext context, IPaymentRep paymentRep)
        {
            _context = context;
            _paymentRep = paymentRep;
        }

        public async Task<RowResultObject<WalletReport>> GetWalletAsync(long customerId, int pageIndex = 1, int pageSize = 20)
        {
            var result = new RowResultObject<WalletReport>();
            try
            {
                var wallet = await GetOrCreateWalletAsync(customerId);
                var transactions = await _context.WalletTransactions
                    .AsNoTracking()
                    .Where(x => x.WalletID == wallet.ID)
                    .OrderByDescending(x => x.TransactionDate)
                    .ThenByDescending(x => x.ID)
                    .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
                    .Take(Math.Max(pageSize, 1))
                    .Select(x => new WalletTransactionReport
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

                result.Result = new WalletReport
                {
                    WalletId = wallet.ID,
                    CustomerId = wallet.CustomerID,
                    Balance = wallet.Balance,
                    Transactions = transactions
                };
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<ListResultObject<AdminWalletTransactionReport>> GetWalletTransactionsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "", string transactionType = "")
        {
            var result = new ListResultObject<AdminWalletTransactionReport>();
            try
            {
                var query = _context.WalletTransactions
                    .Include(x => x.Wallet).ThenInclude(x => x.Customer).ThenInclude(x => x.Person)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(transactionType))
                {
                    query = query.Where(x => x.TransactionType == transactionType);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.ReferenceNumber.Contains(searchText) ||
                        x.Description.Contains(searchText) ||
                        x.Wallet.Customer.Person.FirstName.Contains(searchText) ||
                        x.Wallet.Customer.Person.LastName.Contains(searchText) ||
                        x.Wallet.Customer.Person.PhoneNumber.Contains(searchText));
                }

                result.TotalCount = await query.CountAsync();
                result.PageCount = DbTools.GetPageCount(result.TotalCount, pageSize);
                result.Results = await query
                    .OrderByDescending(x => x.TransactionDate)
                    .ThenByDescending(x => x.ID)
                    .Skip((Math.Max(pageIndex, 1) - 1) * Math.Max(pageSize, 1))
                    .Take(Math.Max(pageSize, 1))
                    .Select(x => new AdminWalletTransactionReport
                    {
                        Id = x.ID,
                        WalletId = x.WalletID,
                        CustomerId = x.Wallet.CustomerID,
                        CustomerName = x.Wallet.Customer.Person.FirstName + " " + x.Wallet.Customer.Person.LastName,
                        CustomerPhoneNumber = x.Wallet.Customer.Person.PhoneNumber,
                        WalletBalance = x.Wallet.Balance,
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
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> ChargeWalletAsync(long customerId, decimal amount, string description = "")
        {
            var result = new BitResultObject();
            try
            {
                if (amount <= 0)
                {
                    result.Status = false;
                    result.ErrorMessage = "مبلغ شارژ کیف پول معتبر نیست";
                    return result;
                }

                var wallet = await GetOrCreateWalletAsync(customerId);
                wallet.Balance += amount;
                wallet.UpdateDate = DateTime.Now.ToShamsi();

                var transaction = new WalletTransaction
                {
                    WalletID = wallet.ID,
                    Amount = amount,
                    TransactionType = "charge",
                    Status = "success",
                    TransactionDate = DateTime.Now.ToShamsi(),
                    ReferenceNumber = Guid.NewGuid().ToString("N"),
                    Description = description,
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi()
                };

                _context.Wallets.Update(wallet);
                await _context.WalletTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();
                result.ID = transaction.ID;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> PayBookingAsync(long customerId, long bookingId, long discountId = 0, string description = "")
        {
            var result = new BitResultObject();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var bookingExists = await _context.Bookings.AnyAsync(x => x.ID == bookingId && x.CustomerID == customerId);
                if (!bookingExists)
                {
                    result.Status = false;
                    result.ErrorMessage = "رزرو برای این مشتری یافت نشد";
                    return result;
                }

                var calcPayment = await _paymentRep.CalculatePaymentAsync(customerId, bookingId, discountId);
                if (!calcPayment.Status || calcPayment.Result == null)
                {
                    result.Status = false;
                    result.ErrorMessage = calcPayment.ErrorMessage;
                    return result;
                }

                var payableAmount = calcPayment.Result.PayedAmount;
                var wallet = await GetOrCreateWalletAsync(customerId);
                if (wallet.Balance < payableAmount)
                {
                    result.Status = false;
                    result.ErrorMessage = "موجودی کیف پول کافی نیست";
                    return result;
                }

                var now = DateTime.Now.ToShamsi();
                var payment = new Payment
                {
                    CreateDate = now,
                    UpdateDate = now,
                    BookingID = bookingId,
                    DepositAmount = calcPayment.Result.DepositAmount,
                    TotalServiceAmount = calcPayment.Result.TotalServiceAmount,
                    PlarformAmount = calcPayment.Result.PlatformAmount,
                    StylistAmount = calcPayment.Result.StylistAmount,
                    AllPaymentAmount = calcPayment.Result.AllPaymentAmount,
                    PayedAmount = calcPayment.Result.PayedAmount,
                    RemainAmount = calcPayment.Result.RemainAmount,
                    DiscountedServiceAmount = calcPayment.Result.DiscountedServiceAmount,
                    VatAmount = calcPayment.Result.VatAmount,
                    PaymentStatus = "wallet",
                    PaymentDate = now,
                    PaymentLevel = 2,
                    PaymentFinished = calcPayment.Result.RemainAmount <= 0,
                    DiscountID = discountId,
                    Description = description,
                    PaymentBookings = new List<PaymentBooking>
                    {
                        new PaymentBooking { BookingID = bookingId }
                    },
                    PaymentDetails = calcPayment.Result.stylistServiceWithDiscountDtos.Select(d => new PaymentDetail
                    {
                        CreateDate = now,
                        UpdateDate = now,
                        Description = description,
                        BookingID = d.BookingID,
                        StylistID = d.StylistID,
                        ServiceManagementID = d.ServiceManagementID,
                        StylistServicePriceVariantID = d.StylistServicePriceVariantID,
                        AppliedOptionSummary = d.AppliedOptionSummary,
                        StylistServiceAmount = d.ServicePrice,
                        DiscountAmount = d.ServicePrice - d.PriceAfterDiscount,
                        DiscountPercent = d.DiscountPercent,
                        OptionValues = d.AppliedOptionValueIDs.Select(optionValueId => new PaymentDetailOptionValue
                        {
                            ServiceOptionValueID = optionValueId
                        }).ToList()
                    }).ToList()
                };

                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                var paymentHistory = new PaymentHistory
                {
                    CreateDate = now,
                    UpdateDate = now,
                    BookingID = bookingId,
                    PaymentID = payment.ID,
                    PaymentMethod = 2,
                    PaymentDate = now,
                    Description = description
                };
                await _context.PaymentHistories.AddAsync(paymentHistory);

                wallet.Balance -= payableAmount;
                wallet.UpdateDate = now;
                _context.Wallets.Update(wallet);

                var walletTransaction = new WalletTransaction
                {
                    CreateDate = now,
                    UpdateDate = now,
                    WalletID = wallet.ID,
                    BookingID = bookingId,
                    PaymentID = payment.ID,
                    Amount = payableAmount,
                    TransactionType = "payment",
                    Status = "success",
                    TransactionDate = now,
                    ReferenceNumber = Guid.NewGuid().ToString("N"),
                    Description = description
                };
                await _context.WalletTransactions.AddAsync(walletTransaction);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                result.ID = payment.ID;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private async Task<Wallet> GetOrCreateWalletAsync(long customerId)
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(x => x.CustomerID == customerId);
            if (wallet != null)
            {
                return wallet;
            }

            wallet = new Wallet
            {
                CustomerID = customerId,
                Balance = 0,
                IsActive = true,
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi()
            };
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }
    }
}
