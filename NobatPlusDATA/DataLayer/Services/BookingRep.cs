using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class BookingRep : IBookingRep
    {

        private NobatPlusContext _context;
        public BookingRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddBookingAsync(Booking Booking)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var bookingServiceIds = Booking.BookingServices
                    .Select(x => x.ServiceManagementID)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                var preparedDuration = await PrepareBookingScheduleAndSnapshotsAsync(Booking);
                var snapshot = await GetBookingDurationSnapshotAsync(Booking.StylistID, bookingServiceIds);
                Booking.ServiceDurationMinutesSnapshot = preparedDuration ?? snapshot.ServiceDurationMinutes;
                Booking.RestTimeMinutesSnapshot = snapshot.RestTimeMinutes;

                bool hasConfilict = await HasBookingConflictForStylistOrCustomerAsync(
                    Booking.StylistID,
                    Booking.CustomerID,
                    Booking.BookingDate,
                    bookingServiceIds,
                    newServiceDurationMinutesOverride: Booking.ServiceDurationMinutesSnapshot,
                    newRestTimeMinutesOverride: Booking.RestTimeMinutesSnapshot,
                   bookingIsCanceled: Booking.IsCancelled);

                if (hasConfilict)
                {
                    throw new Exception("ثبت این نوبت به دلیل وجود تداخل برای مشتری / آرایشگر امکان پذیر نیست");
                }

                await _context.Bookings.AddAsync(Booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.ID = Booking.ID;
                _context.Entry(Booking).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditBookingAsync(Booking Booking)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
                var previousBooking = await _context.Bookings
                    .AsNoTracking()
                    .Include(x => x.BookingServices)
                    .FirstOrDefaultAsync(x => x.ID == Booking.ID);

                if (previousBooking == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "رزرو یافت نشد";
                    return result;
                }

                var bookingServiceIds = Booking.BookingServices
                    .Select(x => x.ServiceManagementID)
                    .Where(x => x > 0)
                    .Distinct()
                    .ToList();

                var preparedDuration = await PrepareBookingScheduleAndSnapshotsAsync(Booking, Booking.ID);

                var previousServiceIds = previousBooking.BookingServices
                    .Select(x => x.ServiceManagementID)
                    .Where(x => x > 0)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();
                var serviceSelectionChanged = !previousServiceIds.SequenceEqual(bookingServiceIds.OrderBy(x => x));
                var durationDefinitionChanged = previousBooking.StylistID != Booking.StylistID || serviceSelectionChanged;

                if (!durationDefinitionChanged &&
                    previousBooking.ServiceDurationMinutesSnapshot.HasValue &&
                    previousBooking.RestTimeMinutesSnapshot.HasValue)
                {
                    Booking.ServiceDurationMinutesSnapshot = previousBooking.ServiceDurationMinutesSnapshot;
                    Booking.RestTimeMinutesSnapshot = previousBooking.RestTimeMinutesSnapshot;
                }
                else
                {
                    var snapshot = await GetBookingDurationSnapshotAsync(Booking.StylistID, bookingServiceIds);
                    Booking.ServiceDurationMinutesSnapshot = preparedDuration ?? snapshot.ServiceDurationMinutes;
                    Booking.RestTimeMinutesSnapshot = snapshot.RestTimeMinutes;
                }

                if (preparedDuration.HasValue)
                {
                    Booking.ServiceDurationMinutesSnapshot = preparedDuration.Value;
                    Booking.RestTimeMinutesSnapshot = Convert.ToInt32((await _context.Stylists.AsNoTracking()
                        .Where(x => x.ID == Booking.StylistID).Select(x => x.RestTime).SingleAsync()).TotalMinutes);
                }

                var bookingTimeChanged = previousBooking.StylistID != Booking.StylistID ||
                                         previousBooking.BookingDate != Booking.BookingDate;

                bool hasConfilict = await HasBookingConflictForStylistOrCustomerAsync(
                    Booking.StylistID,
                    Booking.CustomerID,
                    Booking.BookingDate,
                    bookingServiceIds,
                    Booking.ID,
                    bookingTimeChanged,
                    Booking.ServiceDurationMinutesSnapshot,
                    Booking.RestTimeMinutesSnapshot,
                     bookingIsCanceled: Booking.IsCancelled);

                if (hasConfilict)
                {
                    throw new Exception("ثبت این نوبت به دلیل وجود تداخل برای مشتری / آرایشگر امکان پذیر نیست");
                }

                var nextServices = Booking.BookingServices?.ToList() ?? new List<BookingService>();

                foreach (var bookingService in nextServices)
                {
                    bookingService.BookingID = Booking.ID;
                    foreach (var optionValue in bookingService.OptionValues ?? new List<BookingServiceOptionValue>())
                    {
                        optionValue.BookingID = Booking.ID;
                        optionValue.ServiceManagementID = bookingService.ServiceManagementID;
                    }
                }

                var oldServices = await _context.BookingServices
                    .Where(x => x.BookingID == Booking.ID)
                    .ToListAsync();

                if (oldServices.Any())
                {
                    _context.BookingServices.RemoveRange(oldServices);
                    await _context.SaveChangesAsync();
                }

                Booking.BookingServices = new List<BookingService>();
                _context.Bookings.Update(Booking);
                if (!previousBooking.IsCancelled && Booking.IsCancelled)
                {
                    await RefundWalletPaymentsForCancelledBookingAsync(Booking.ID);
                    await ReverseStylistEarningsForCancelledBookingAsync(Booking.ID);
                }

                if (nextServices.Any())
                {
                    await _context.BookingServices.AddRangeAsync(nextServices);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.ID = Booking.ID;
                _context.Entry(Booking).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        private async Task RefundWalletPaymentsForCancelledBookingAsync(long bookingId)
        {
            var now = DateTime.Now.ToShamsi();
            var walletPayments = await _context.WalletTransactions
                .Include(x => x.Wallet)
                .Include(x => x.Payment)
                .Where(x =>
                    x.BookingID == bookingId &&
                    x.TransactionType == "payment" &&
                    x.Status == "success")
                .ToListAsync();

            foreach (var paymentTransaction in walletPayments)
            {
                var reference = $"refund:{paymentTransaction.ID}";
                var alreadyRefunded = await _context.WalletTransactions
                    .AnyAsync(x => x.ReferenceNumber == reference && x.TransactionType == "refund");

                if (alreadyRefunded)
                {
                    continue;
                }

                paymentTransaction.Wallet.Balance += paymentTransaction.Amount;
                paymentTransaction.Wallet.UpdateDate = now;
                _context.Wallets.Update(paymentTransaction.Wallet);

                await _context.WalletTransactions.AddAsync(new WalletTransaction
                {
                    CreateDate = now,
                    UpdateDate = now,
                    WalletID = paymentTransaction.WalletID,
                    BookingID = bookingId,
                    PaymentID = paymentTransaction.PaymentID,
                    Amount = paymentTransaction.Amount,
                    TransactionType = "refund",
                    Status = "success",
                    TransactionDate = now,
                    ReferenceNumber = reference,
                    Description = "برگشت وجه رزرو لغو شده"
                });

                if (paymentTransaction.Payment != null)
                {
                    paymentTransaction.Payment.PaymentStatus = "refunded";
                    paymentTransaction.Payment.UpdateDate = now;
                    _context.Payments.Update(paymentTransaction.Payment);
                }
            }
        }

        private async Task ReverseStylistEarningsForCancelledBookingAsync(long bookingId)
        {
            var earnings = await _context.FinancialTransactions
                .Include(x => x.FinancialAccount)
                .Where(x =>
                    x.BookingID == bookingId &&
                    x.TransactionType == "earning" &&
                    x.Status == "success")
                .ToListAsync();

            var now = DateTime.Now.ToShamsi();
            foreach (var earning in earnings)
            {
                var reference = $"cancellation-reversal:{earning.ID}";
                var alreadyReversed = await _context.FinancialTransactions
                    .AnyAsync(x => x.ReferenceNumber == reference && x.TransactionType == "cancellation_reversal");
                if (alreadyReversed)
                    continue;

                earning.FinancialAccount.Balance -= earning.Amount;
                earning.FinancialAccount.UpdateDate = now;
                _context.FinancialAccounts.Update(earning.FinancialAccount);

                await _context.FinancialTransactions.AddAsync(new FinancialTransaction
                {
                    CreateDate = now,
                    UpdateDate = now,
                    FinancialAccountID = earning.FinancialAccountID,
                    BookingID = bookingId,
                    PaymentID = earning.PaymentID,
                    Amount = earning.Amount,
                    TransactionType = "cancellation_reversal",
                    Status = "success",
                    TransactionDate = now,
                    ReferenceNumber = reference,
                    Description = "اصلاح درآمد نوبت لغوشده"
                });
            }
        }

        public async Task<BitResultObject> ExistBookingAsync(long BookingId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Bookings.AsNoTracking().AnyAsync(x => x.ID == BookingId);
                result.ID = BookingId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
             
        }


        public async Task<ListResultObject<BookingDTO>> GetAllBookingsAsync(
            long serviceManagementId = 0,
            long customerId = 0,
            long stylistId = 0,
            int cancelState = 0,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageIndex = 1,
            int pageSize = 20,
            string searchText = "",
            string sortQuery = "",
            string status = "")
        {
            ListResultObject<BookingDTO> results = new();

            try
            {
                IQueryable<Booking> bookingsQuery;

                if (serviceManagementId > 0)
                {
                    bookingsQuery = _context.BookingServices
                        .Where(bs => bs.ServiceManagementID == serviceManagementId)
                        .Select(bs => bs.Booking).Include(b=> b.Stylist).ThenInclude(b=> b.Person).ThenInclude(b=> b.Address)
                        .AsNoTracking();
                }
                else
                {
                    bookingsQuery = _context.Bookings.Include(b => b.Stylist).ThenInclude(b => b.Person).ThenInclude(b => b.Address)
                        .AsNoTracking();
                }

                if (customerId > 0)
                    bookingsQuery = bookingsQuery.Where(x => x.CustomerID == customerId);

                if (stylistId > 0)
                    bookingsQuery = bookingsQuery.Where(x => x.StylistID == stylistId);

                if (cancelState == 1)
                    bookingsQuery = bookingsQuery.Where(x => x.IsCancelled);
                else if (cancelState == 2)
                    bookingsQuery = bookingsQuery.Where(x => !x.IsCancelled);

                if (!string.IsNullOrWhiteSpace(status))
                {
                    var normalizedStatus = status.Trim();
                    bookingsQuery = bookingsQuery.Where(x => x.Status == normalizedStatus);
                }

                if (fromDate != null)
                {
                    var from = fromDate.Value;
                    bookingsQuery = bookingsQuery.Where(x => x.BookingDate >= from);
                }

                if (toDate != null)
                {
                    var to = toDate.Value;
                    bookingsQuery = bookingsQuery.Where(x => x.BookingDate <= to);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    searchText = searchText.Trim();
                    bookingsQuery = bookingsQuery.Where(x =>
                        x.ID.ToString().Contains(searchText) ||
                        x.Stylist.Person.FirstName.Contains(searchText) ||
                        x.Stylist.Person.LastName.Contains(searchText) ||
                        x.Stylist.Person.PhoneNumber.Contains(searchText) ||
                        x.Customer.Person.FirstName.Contains(searchText) ||
                        x.Customer.Person.LastName.Contains(searchText) ||
                        x.Customer.Person.PhoneNumber.Contains(searchText) ||
                        x.Status.Contains(searchText) ||
                        (x.Description != null && x.Description.Contains(searchText)) ||
                        x.BookingServices.Any(bs => bs.ServiceManagement.ServiceName.Contains(searchText)));
                }

                bookingsQuery = bookingsQuery
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .AsNoTracking();

                results.TotalCount = await bookingsQuery.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await (
                    from b in bookingsQuery

                    let totalDurationMinutes =
                        (
                            from bs in _context.BookingServices
                            join ss in _context.StylistServices
                                on new { b.StylistID, bs.ServiceManagementID }
                                equals new { ss.StylistID, ss.ServiceManagementID }
                            where bs.BookingID == b.ID
                            select ss.ServiceDuration == null
                                ? (int?)0
                                : EF.Functions.DateDiffMinute(
                                    TimeSpan.Zero,
                                    ss.ServiceDuration
                                )
                        ).Sum() ?? 0

                    let restMinutes =
                        b.Stylist.RestTime == null
                            ? 0
                            : EF.Functions.DateDiffMinute(
                                TimeSpan.Zero,
                                b.Stylist.RestTime
                            )

                    let isManualBooking = b.Stylist.BookingCreationMode != null &&
                        b.Stylist.BookingCreationMode.ToLower() == "manual"
                    let manualSlotMinutes = b.Stylist.SlotIntervalMinutes >= 5 &&
                        b.Stylist.SlotIntervalMinutes <= 240
                            ? b.Stylist.SlotIntervalMinutes
                            : 30
                    let effectiveDurationMinutes = b.ServiceDurationMinutesSnapshot ??
                        (isManualBooking ? manualSlotMinutes : totalDurationMinutes)
                    let effectiveRestMinutes = b.RestTimeMinutesSnapshot ?? restMinutes

                    orderby b.CreateDate descending

                    select new BookingDTO
                    {
                        ID = b.ID,
                        StylistID = b.StylistID,
                        CustomerID = b.CustomerID,

                        CreateDate = b.CreateDate,
                        UpdateDate = b.UpdateDate,
                        Description = b.Description,

                        BookingStartDate = b.BookingDate,

                        TotalDurationMinutes = effectiveDurationMinutes,

                        BookingEndDate = b.BookingDate.AddMinutes(effectiveDurationMinutes),

                        TotalBlockMinutes = effectiveDurationMinutes + effectiveRestMinutes,
                        ServiceIDs = _context.BookingServices
                            .Where(bs => bs.BookingID == b.ID)
                            .Select(bs => bs.ServiceManagementID)
                            .ToList(),

                        Status = b.Status,
                        IsCancelled = b.IsCancelled,
                        CancelReason = b.CancelReason,
                        ScheduleBlockID = b.ScheduleBlockID,

                        Stylist = b.Stylist,
                        StylistAddress = b.Stylist.Person.Address,
                        Customer = b.Customer,
                        Services = b.BookingServices.Select(bs => new BookingServiceSelectionDTO
                        {
                            ServiceID = bs.ServiceManagementID,
                            ServiceName = bs.ServiceManagement.ServiceName,
                            OptionValueIDs = bs.OptionValues.Select(ov => ov.ServiceOptionValueID).ToList(),
                            StylistServicePriceVariantID = bs.StylistServicePriceVariantID,
                            UnitPriceSnapshot = bs.UnitPriceSnapshot,
                            DiscountPercentSnapshot = bs.DiscountPercentSnapshot,
                            PriceAfterDiscountSnapshot = bs.PriceAfterDiscountSnapshot,
                            DepositPercentSnapshot = bs.DepositPercentSnapshot,
                            DurationMinutesSnapshot = bs.DurationMinutesSnapshot,
                            OptionValues = bs.OptionValues.Select(ov => new BookingServiceOptionValueDTO
                            {
                                ServiceOptionValueID = ov.ServiceOptionValueID,
                                ServiceOptionID = ov.ServiceOptionValue.ServiceOptionID,
                                OptionName = ov.ServiceOptionValue.ServiceOption.OptionName,
                                ValueName = ov.ServiceOptionValue.ValueName
                            }).ToList()
                        }).ToList()
                    }
                )
                .SortBy(sortQuery)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();

                NormalizeBookingServiceSelections(results.Results);

                results.Status = true;
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return results;
        }

        public async Task<RowResultObject<BookingDTO>> GetBookingByIdAsync(long bookingId)
        {
            RowResultObject<BookingDTO> result = new();

            try
            {
                var bookingQuery = _context.Bookings
                    .Include(x => x.Stylist).ThenInclude(x => x.Person).ThenInclude(b=> b.Address)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .AsNoTracking()
                    .Where(x => x.ID == bookingId);

                result.Result = await (
                    from b in bookingQuery

                    let totalDurationMinutes =
                        (
                            from bs in _context.BookingServices
                            join ss in _context.StylistServices
                                on new { b.StylistID, bs.ServiceManagementID }
                                equals new { ss.StylistID, ss.ServiceManagementID }
                            where bs.BookingID == b.ID
                            select ss.ServiceDuration == null
                                ? (int?)0
                                : EF.Functions.DateDiffMinute(
                                    TimeSpan.Zero,
                                    ss.ServiceDuration
                                )
                        ).Sum() ?? 0

                    let restMinutes =
                        (
                            b.Stylist.RestTime == null
                                ? (int?)0
                                : EF.Functions.DateDiffMinute(
                                    TimeSpan.Zero,
                                    b.Stylist.RestTime
                                )
                        ) ?? 0

                    let isManualBooking = b.Stylist.BookingCreationMode != null &&
                        b.Stylist.BookingCreationMode.ToLower() == "manual"
                    let manualSlotMinutes = b.Stylist.SlotIntervalMinutes >= 5 &&
                        b.Stylist.SlotIntervalMinutes <= 240
                            ? b.Stylist.SlotIntervalMinutes
                            : 30
                    let effectiveDurationMinutes = b.ServiceDurationMinutesSnapshot ??
                        (isManualBooking ? manualSlotMinutes : totalDurationMinutes)
                    let effectiveRestMinutes = b.RestTimeMinutesSnapshot ?? restMinutes

                    select new BookingDTO
                    {
                        ID = b.ID,
                        StylistID = b.StylistID,
                        CustomerID = b.CustomerID,
                        StylistAddress = b.Stylist.Person.Address,
                        CreateDate = b.CreateDate,
                        UpdateDate = b.UpdateDate,
                        Description = b.Description,

                        BookingStartDate = b.BookingDate,

                        TotalDurationMinutes = effectiveDurationMinutes,

                        BookingEndDate = b.BookingDate.AddMinutes(effectiveDurationMinutes),

                        TotalBlockMinutes = effectiveDurationMinutes + effectiveRestMinutes,
                        ServiceIDs = _context.BookingServices
                            .Where(bs => bs.BookingID == b.ID)
                            .Select(bs => bs.ServiceManagementID)
                            .ToList(),

                        Status = b.Status,
                        IsCancelled = b.IsCancelled,
                        CancelReason = b.CancelReason,
                        ScheduleBlockID = b.ScheduleBlockID,

                        Stylist = b.Stylist,
                        Customer = b.Customer,
                        Services = b.BookingServices.Select(bs => new BookingServiceSelectionDTO
                        {
                            ServiceID = bs.ServiceManagementID,
                            ServiceName = bs.ServiceManagement.ServiceName,
                            OptionValueIDs = bs.OptionValues.Select(ov => ov.ServiceOptionValueID).ToList(),
                            StylistServicePriceVariantID = bs.StylistServicePriceVariantID,
                            UnitPriceSnapshot = bs.UnitPriceSnapshot,
                            DiscountPercentSnapshot = bs.DiscountPercentSnapshot,
                            PriceAfterDiscountSnapshot = bs.PriceAfterDiscountSnapshot,
                            DepositPercentSnapshot = bs.DepositPercentSnapshot,
                            DurationMinutesSnapshot = bs.DurationMinutesSnapshot,
                            OptionValues = bs.OptionValues.Select(ov => new BookingServiceOptionValueDTO
                            {
                                ServiceOptionValueID = ov.ServiceOptionValueID,
                                ServiceOptionID = ov.ServiceOptionValue.ServiceOptionID,
                                OptionName = ov.ServiceOptionValue.ServiceOption.OptionName,
                                ValueName = ov.ServiceOptionValue.ValueName
                            }).ToList()
                        }).ToList()
                    }
                ).SingleOrDefaultAsync();

                NormalizeBookingServiceSelections(result.Result == null ? new List<BookingDTO>() : new List<BookingDTO> { result.Result });

                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }


        private static void NormalizeBookingServiceSelections(List<BookingDTO> bookings)
        {
            foreach (var booking in bookings)
            {
                if (booking.Services == null || !booking.Services.Any())
                {
                    booking.Services = null;
                    continue;
                }

                foreach (var service in booking.Services)
                {
                    if (service.OptionValueIDs == null || !service.OptionValueIDs.Any())
                    {
                        service.OptionValueIDs = null;
                    }

                    if (service.OptionValues == null || !service.OptionValues.Any())
                    {
                        service.OptionValues = null;
                    }
                }
            }
        }



        public async Task<BitResultObject> RemoveBookingAsync(Booking Booking)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Bookings.Remove(Booking);
                await _context.SaveChangesAsync();
                result.ID = Booking.ID;
                _context.Entry(Booking).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveBookingAsync(long BookingId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var BookingDto = await GetBookingByIdAsync(BookingId);
                var theBooking = new Booking()
                {
                    BookingDate = BookingDto.Result.BookingStartDate,
                    CancelReason = BookingDto.Result.CancelReason,
                    CreateDate = BookingDto.Result.CreateDate,
                    Customer = BookingDto.Result.Customer,
                    CustomerID = BookingDto.Result.CustomerID,
                    Description = BookingDto.Result.Description,
                    ID = BookingDto.Result.ID,
                    UpdateDate = BookingDto.Result.UpdateDate,
                    IsCancelled = BookingDto.Result.IsCancelled,
                    Status = BookingDto.Result.Status,
                    Stylist = BookingDto.Result.Stylist,
                    StylistID = BookingDto.Result.StylistID,
                };
                result = await RemoveBookingAsync(theBooking);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
          
        }

        private async Task<int?> PrepareBookingScheduleAndSnapshotsAsync(Booking booking, long excludedBookingId = 0)
        {
            if (booking.BookingServices == null || !booking.BookingServices.Any())
                throw new InvalidOperationException("حداقل یک خدمت باید برای نوبت انتخاب شود.");

            var stylist = await _context.Stylists.AsNoTracking()
                .Where(x => x.ID == booking.StylistID)
                .Select(x => new { x.BookingCreationMode, x.SlotIntervalMinutes })
                .SingleOrDefaultAsync() ?? throw new InvalidOperationException("آرایشگر یافت نشد.");

            var isScheduleMode = IsManualScheduleMode(stylist.BookingCreationMode);
            StylistScheduleBlock? block = null;
            if (booking.ScheduleBlockID.HasValue)
            {
                block = await _context.StylistScheduleBlocks
                    .Include(x => x.StylistServicePriceVariant).ThenInclude(x => x.OptionValues)
                    .SingleOrDefaultAsync(x => x.ID == booking.ScheduleBlockID.Value)
                    ?? throw new InvalidOperationException("بازه زمانی انتخاب‌شده یافت نشد.");

                if (block.StylistID != booking.StylistID) throw new InvalidOperationException("بازه زمانی متعلق به این آرایشگر نیست.");
                if (!block.IsActive || !block.IsBookable) throw new InvalidOperationException("بازه زمانی انتخاب‌شده قابل رزرو نیست.");
                if (await _context.Bookings.AnyAsync(x => x.ScheduleBlockID == block.ID && x.ID != excludedBookingId && !x.IsCancelled))
                    throw new InvalidOperationException("این بازه زمانی قبلاً رزرو شده است.");

                booking.BookingDate = block.StartDateTime;
                if (block.ServiceManagementID.HasValue)
                {
                    if (booking.BookingServices.Count != 1 || booking.BookingServices.Single().ServiceManagementID != block.ServiceManagementID.Value)
                        throw new InvalidOperationException("خدمت انتخاب‌شده با خدمت تعریف‌شده برای این بازه مطابقت ندارد.");
                }
            }
            else if (isScheduleMode && !booking.IsCancelled)
            {
                throw new InvalidOperationException("برای این آرایشگر انتخاب بازه برنامه دستی الزامی است.");
            }

            var totalDuration = 0;
            foreach (var bookingService in booking.BookingServices)
            {
                var service = await _context.StylistServices.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.StylistID == booking.StylistID && x.ServiceManagementID == bookingService.ServiceManagementID)
                    ?? throw new InvalidOperationException("یک یا چند خدمت برای این آرایشگر تعریف نشده است.");

                var optionIds = (bookingService.OptionValues ?? new List<BookingServiceOptionValue>())
                    .Select(x => x.ServiceOptionValueID).Where(x => x > 0).Distinct().OrderBy(x => x).ToList();
                StylistServicePriceVariant? variant = null;
                if (block?.StylistServicePriceVariantID != null && block.ServiceManagementID == bookingService.ServiceManagementID)
                {
                    variant = await _context.StylistServicePriceVariants.AsNoTracking().Include(x => x.OptionValues)
                        .SingleAsync(x => x.ID == block.StylistServicePriceVariantID.Value);
                    var variantOptions = variant.OptionValues.Select(x => x.ServiceOptionValueID).OrderBy(x => x).ToList();
                    if (!variantOptions.SequenceEqual(optionIds))
                        throw new InvalidOperationException("گزینه‌های خدمت با قیمت متغیر تعریف‌شده برای این بازه مطابقت ندارند.");
                }
                else if (service.HasDynamicPricing)
                {
                    if (bookingService.StylistServicePriceVariantID.HasValue)
                    {
                        variant = await _context.StylistServicePriceVariants.AsNoTracking().Include(x => x.OptionValues)
                            .SingleOrDefaultAsync(x => x.ID == bookingService.StylistServicePriceVariantID.Value &&
                                x.StylistID == booking.StylistID &&
                                x.ServiceManagementID == bookingService.ServiceManagementID &&
                                x.IsActive && x.BookingTagID == null);

                        if (variant != null)
                        {
                            var variantOptions = variant.OptionValues.Select(x => x.ServiceOptionValueID).OrderBy(x => x).ToList();
                            if (!variantOptions.SequenceEqual(optionIds))
                                throw new InvalidOperationException("گزینه‌های خدمت با قیمت متغیر انتخاب‌شده مطابقت ندارند.");
                        }
                    }
                    else
                    {
                        var key = StylistServicePriceVariant.BuildOptionValueCombinationKey(optionIds);
                        variant = await _context.StylistServicePriceVariants.AsNoTracking().Include(x => x.OptionValues)
                            .Where(x => x.StylistID == booking.StylistID && x.ServiceManagementID == bookingService.ServiceManagementID && x.OptionValueCombinationKey == key && x.IsActive && x.BookingTagID == null)
                            .FirstOrDefaultAsync();
                    }
                    if (variant == null) throw new InvalidOperationException("برای ترکیب گزینه‌های انتخاب‌شده قیمت فعالی تعریف نشده است.");
                }

                var basePrice = variant?.Price ?? service.ServicePrice;
                var depositPercent = variant?.DepositPercent ?? service.DepositPercent;
                var durationMinutes = Convert.ToInt32((variant?.Duration ?? service.ServiceDuration).TotalMinutes);
                if (block != null && block.ServiceManagementID == bookingService.ServiceManagementID)
                {
                    basePrice = block.PriceOverride ?? basePrice;
                    depositPercent = block.DepositPercentOverride ?? depositPercent;
                }
                var discountPercent = await GetApplicableDiscountPercentAsync(booking.StylistID, bookingService.ServiceManagementID, booking.CustomerID);

                bookingService.StylistServicePriceVariantID = variant?.ID;
                bookingService.UnitPriceSnapshot = basePrice;
                bookingService.DiscountPercentSnapshot = discountPercent;
                bookingService.PriceAfterDiscountSnapshot = basePrice * (1m - discountPercent / 100m);
                bookingService.DepositPercentSnapshot = depositPercent;
                bookingService.DurationMinutesSnapshot = durationMinutes;
                totalDuration += durationMinutes;
            }

            if (block != null) return Math.Max(1, Convert.ToInt32((block.EndDateTime - block.StartDateTime).TotalMinutes));
            if (string.Equals(stylist.BookingCreationMode, "manual", StringComparison.OrdinalIgnoreCase))
                return NormalizeSlotIntervalMinutes(stylist.SlotIntervalMinutes);
            return Math.Max(1, totalDuration);
        }

        public async Task<ListResultObject<PublicBookingSlotDTO>> GetAvailableBookingSlotsAsync(
            long stylistId, long customerId, DateTime fromDate, DateTime toDate, List<BookingServiceSelectionDTO> services)
        {
            var result = new ListResultObject<PublicBookingSlotDTO>();
            try
            {
                var stylist = await _context.Stylists.AsNoTracking()
                    .Where(x => x.ID == stylistId)
                    .Select(x => new { x.ID, x.BookingCreationMode, x.SlotDisplayMode, x.SlotIntervalMinutes, x.RestTime })
                    .SingleOrDefaultAsync();
                if (stylist == null) throw new InvalidOperationException("آرایشگر یافت نشد.");
                if (IsManualScheduleMode(stylist.BookingCreationMode)) throw new InvalidOperationException("زمان‌های این آرایشگر باید از برنامه دستی دریافت شوند.");

                services = services?.Where(x => x.ServiceID > 0).GroupBy(x => x.ServiceID).Select(x => x.First()).ToList() ?? new();
                if (!services.Any()) throw new InvalidOperationException("حداقل یک خدمت برای محاسبه زمان‌های آزاد الزامی است.");

                var totalDuration = 0;
                var totalPrice = 0m;
                var totalDiscountedPrice = 0m;
                var depositAmount = 0m;
                foreach (var selected in services)
                {
                    var service = await _context.StylistServices.AsNoTracking().SingleOrDefaultAsync(x => x.StylistID == stylistId && x.ServiceManagementID == selected.ServiceID)
                        ?? throw new InvalidOperationException("یک یا چند خدمت برای این آرایشگر تعریف نشده است.");
                    var price = service.ServicePrice;
                    var duration = service.ServiceDuration;
                    var deposit = service.DepositPercent;
                    if (service.HasDynamicPricing && selected.StylistServicePriceVariantID.HasValue)
                    {
                        var selectedVariant = await _context.StylistServicePriceVariants.AsNoTracking()
                            .Include(x => x.OptionValues)
                            .SingleOrDefaultAsync(x => x.ID == selected.StylistServicePriceVariantID.Value &&
                                x.StylistID == stylistId && x.ServiceManagementID == selected.ServiceID &&
                                x.IsActive && x.BookingTagID == null);
                        if (selectedVariant == null)
                            throw new InvalidOperationException("قیمت متغیر انتخاب‌شده معتبر نیست.");

                        var optionIds = (selected.OptionValueIDs ?? new List<long>())
                            .Where(x => x > 0).Distinct().OrderBy(x => x).ToList();
                        var variantOptionIds = selectedVariant.OptionValues.Select(x => x.ServiceOptionValueID)
                            .OrderBy(x => x).ToList();
                        if (!variantOptionIds.SequenceEqual(optionIds))
                            throw new InvalidOperationException("گزینه‌های خدمت با قیمت متغیر انتخاب‌شده مطابقت ندارند.");

                        price = selectedVariant.Price;
                        duration = selectedVariant.Duration;
                        deposit = selectedVariant.DepositPercent;
                    }
                    else if (service.HasDynamicPricing)
                    {
                        var key = StylistServicePriceVariant.BuildOptionValueCombinationKey(selected.OptionValueIDs);
                        var variant = await _context.StylistServicePriceVariants.AsNoTracking()
                            .Where(x => x.StylistID == stylistId && x.ServiceManagementID == selected.ServiceID && x.OptionValueCombinationKey == key && x.IsActive && x.BookingTagID == null)
                            .FirstOrDefaultAsync()
                            ?? throw new InvalidOperationException("برای گزینه‌های انتخاب‌شده قیمت متغیر فعالی تعریف نشده است.");
                        price = variant.Price; duration = variant.Duration; deposit = variant.DepositPercent;
                    }
                    var discount = await GetApplicableDiscountPercentAsync(stylistId, selected.ServiceID, customerId);
                    var discounted = price * (1m - discount / 100m);
                    totalDuration += Convert.ToInt32(duration.TotalMinutes);
                    totalPrice += price;
                    totalDiscountedPrice += discounted;
                    depositAmount += discounted * deposit / 100m;
                }

                var isFixed = string.Equals(stylist.BookingCreationMode, "manual", StringComparison.OrdinalIgnoreCase);
                if (isFixed) totalDuration = NormalizeSlotIntervalMinutes(stylist.SlotIntervalMinutes);
                var restMinutes = Math.Max(0, Convert.ToInt32(stylist.RestTime.TotalMinutes));
                var displayStep = isFixed ? totalDuration + restMinutes : NormalizeSlotIntervalMinutes(stylist.SlotIntervalMinutes);
                var now = DateTime.Now.ToShamsi();

                var workTimes = await _context.WorkTimes.AsNoTracking().Where(x => x.StylistID == stylistId).ToListAsync();
                var leaves = await _context.StylistPacifics.AsNoTracking().Where(x => x.StylistID == stylistId && x.PacificStartDate < toDate && x.PacificEndDate > fromDate).ToListAsync();
                var bookings = await _context.Bookings.AsNoTracking()
                    .Where(x => !x.IsCancelled && (x.StylistID == stylistId || (customerId > 0 && x.CustomerID == customerId)) && x.BookingDate < toDate.AddDays(1) && x.BookingDate >= fromDate.AddDays(-1))
                    .Select(x => new { x.BookingDate, Duration = x.ServiceDurationMinutesSnapshot ?? 30, Rest = x.RestTimeMinutesSnapshot ?? 0 }).ToListAsync();

                var slots = new List<PublicBookingSlotDTO>();
                for (var day = fromDate.Date; day <= toDate.Date; day = day.AddDays(1))
                {
                    var daySlots = new List<PublicBookingSlotDTO>();
                    foreach (var work in workTimes.Where(x => MatchDayOfWeek(x.DayOfWeek, day.DayOfWeek)).OrderBy(x => x.WorkStartTime))
                    {
                        var cursor = day.Add(work.WorkStartTime);
                        var workEnd = day.Add(work.WorkEndTime);
                        while (cursor.AddMinutes(totalDuration) <= workEnd)
                        {
                            var serviceEnd = cursor.AddMinutes(totalDuration);
                            var blockEnd = serviceEnd.AddMinutes(restMinutes);
                            var inRange = cursor >= fromDate && cursor <= toDate && cursor >= now;
                            var leaveConflict = leaves.Any(x => x.PacificStartDate < blockEnd && x.PacificEndDate > cursor);
                            var bookingConflict = bookings.Any(x => x.BookingDate < blockEnd && x.BookingDate.AddMinutes(x.Duration + x.Rest) > cursor);
                            if (inRange && !leaveConflict && !bookingConflict)
                            {
                                var discountPercent = totalPrice <= 0 ? 0 : Convert.ToInt32(Math.Round((totalPrice - totalDiscountedPrice) * 100m / totalPrice));
                                daySlots.Add(new PublicBookingSlotDTO
                                {
                                    StylistID = stylistId, BookingStartDate = cursor, BookingEndDate = serviceEnd,
                                    TotalDurationMinutes = totalDuration, TotalBlockMinutes = totalDuration + restMinutes,
                                    ServiceIDs = services.Select(x => x.ServiceID).ToList(), BookingCreationMode = stylist.BookingCreationMode,
                                    ServicePrice = totalPrice, DiscountPercent = discountPercent, PriceAfterDiscount = totalDiscountedPrice,
                                    DepositPercent = totalDiscountedPrice <= 0 ? 0 : Convert.ToInt32(Math.Round(depositAmount * 100m / totalDiscountedPrice))
                                });
                            }
                            cursor = cursor.AddMinutes(displayStep);
                        }
                    }
                    if ((stylist.SlotDisplayMode ?? "").Contains("first", StringComparison.OrdinalIgnoreCase) && daySlots.Any())
                        slots.Add(daySlots.OrderBy(x => x.BookingStartDate).First());
                    else
                        slots.AddRange(daySlots);
                }
                result.Results = slots.OrderBy(x => x.BookingStartDate).ToList();
                result.TotalCount = result.Results.Count;
                result.PageCount = result.TotalCount > 0 ? 1 : 0;
            }
            catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
            return result;
        }

        private async Task<int> GetApplicableDiscountPercentAsync(long stylistId, long serviceId, long customerId)
        {
            var now = DateTime.Now.ToShamsi();
            var service = from sd in _context.ServiceDiscounts join d in _context.Discounts on sd.DiscountId equals d.ID where sd.ServiceManagementId == serviceId && (sd.StylistId == null || sd.StylistId <= 0 || sd.StylistId == stylistId) && d.StartDate <= now && d.EndDate >= now && !d.CodeRequired select d.DiscountAmount;
            var customer = from cd in _context.CustomerDiscounts join d in _context.Discounts on cd.DiscountId equals d.ID where customerId > 0 && cd.CustomerId == customerId && (cd.StylistId <= 0 || cd.StylistId == stylistId) && d.StartDate <= now && d.EndDate >= now && !d.CodeRequired select d.DiscountAmount;
            var assignment = from da in _context.DiscountAssignments join d in _context.Discounts on da.DiscountId equals d.ID where (da.StylistId == stylistId || ((da.StylistId == null || da.StylistId <= 0) && da.AdminId != null && da.AdminId > 0)) && d.StartDate <= now && d.EndDate >= now && !d.CodeRequired select d.DiscountAmount;
            return Math.Clamp(await service.Concat(customer).Concat(assignment).Select(x => (int?)x).MaxAsync() ?? 0, 0, 100);
        }

        private static bool IsManualScheduleMode(string? mode) =>
            string.Equals(mode, "manual-schedule", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(mode, "manualschedule", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(mode, "schedule", StringComparison.OrdinalIgnoreCase);

        private async Task<(int ServiceDurationMinutes, int RestTimeMinutes)> GetBookingDurationSnapshotAsync(
            long stylistId,
            IReadOnlyCollection<long> serviceManagementIds)
        {
            if (serviceManagementIds == null || serviceManagementIds.Count == 0)
                throw new ArgumentException("حداقل یک سرویس باید انتخاب شود.", nameof(serviceManagementIds));

            var stylist = await _context.Stylists
                .Where(x => x.ID == stylistId)
                .Select(x => new
                {
                    x.RestTime,
                    x.SlotIntervalMinutes,
                    x.BookingCreationMode
                })
                .SingleOrDefaultAsync();

            if (stylist == null)
                throw new ArgumentException("آرایشگر یافت نشد.", nameof(stylistId));

            var serviceDurations = await _context.StylistServices
                .Where(x => x.StylistID == stylistId && serviceManagementIds.Contains(x.ServiceManagementID))
                .Select(x => new { x.ServiceManagementID, x.ServiceDuration })
                .ToListAsync();

            if (serviceDurations.Select(x => x.ServiceManagementID).Distinct().Count() != serviceManagementIds.Count)
                throw new ArgumentException("یک یا چند سرویس برای این آرایشگر تعریف نشده است.", nameof(serviceManagementIds));

            if (string.Equals(stylist.BookingCreationMode, "manual", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    NormalizeSlotIntervalMinutes(stylist.SlotIntervalMinutes),
                    GetMinutes(stylist.RestTime));
            }

            return (
                serviceDurations
                    .GroupBy(x => x.ServiceManagementID)
                    .Sum(group => GetMinutes(group.First().ServiceDuration)),
                GetMinutes(stylist.RestTime));
        }

        public async Task<bool> HasBookingConflictForStylistOrCustomerAsync(
    long stylistId,
    long customerId,
    DateTime newStart,
    List<long> serviceManagementIds,
    long bookingId = 0,
    bool validateSlotAlignment = true,
    int? newServiceDurationMinutesOverride = null,
    int? newRestTimeMinutesOverride = null,
            bool bookingIsCanceled = false)
        {
            if (bookingIsCanceled)
            {
                return false;
            }
            if (serviceManagementIds == null || serviceManagementIds.Count == 0)
                throw new ArgumentException("حداقل یک سرویس باید انتخاب شود.", nameof(serviceManagementIds));

            serviceManagementIds = serviceManagementIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var stylist = await _context.Stylists.AsNoTracking()
                .Where(s => s.ID == stylistId)
                .Select(s => new { s.ID, s.RestTime, s.SlotIntervalMinutes, s.BookingCreationMode })
                .SingleOrDefaultAsync();

            if (stylist == null)
                throw new ArgumentException("آرایشگر یافت نشد.", nameof(stylistId));

            var isManualBooking = string.Equals(
                stylist.BookingCreationMode,
                "manual",
                StringComparison.OrdinalIgnoreCase);
            var manualSlotMinutes = NormalizeSlotIntervalMinutes(stylist.SlotIntervalMinutes);
            var restMinutes = newRestTimeMinutesOverride ?? GetMinutes(stylist.RestTime);

            var selectedServiceDurations = await _context.StylistServices.AsNoTracking()
                .Where(ss => ss.StylistID == stylistId && serviceManagementIds.Contains(ss.ServiceManagementID))
                .Select(ss => ss.ServiceDuration)
                .ToListAsync();

            if (selectedServiceDurations.Count != serviceManagementIds.Count)
                throw new ArgumentException("یک یا چند سرویس برای این آرایشگر تعریف نشده است.", nameof(serviceManagementIds));

            var newDurationMinutes = isManualBooking
                ? manualSlotMinutes
                : newServiceDurationMinutesOverride ?? selectedServiceDurations.Sum(GetMinutes);
            var newServiceEnd = newStart.AddMinutes(newDurationMinutes);
            var newBlockEnd = newServiceEnd.AddMinutes(restMinutes);

            await EnsureBookingIsInsideWorkTimeAsync(stylistId, newStart, newServiceEnd);
            if (validateSlotAlignment && isManualBooking)
            {
                await EnsureBookingStartMatchesFixedIntervalAsync(
                    stylistId,
                    newStart,
                    manualSlotMinutes,
                    restMinutes);
            }
            await EnsureBookingIsOutsideStylistPacificAsync(stylistId, newStart, newBlockEnd);

            var existingBookingsQuery = _context.Bookings.AsNoTracking()
                .Where(b => (b.StylistID == stylistId) || (b.CustomerID == customerId))
                .Where(b => !b.IsCancelled);

            if (bookingId > 0)
            {
                existingBookingsQuery = existingBookingsQuery.Where(b => b.ID != bookingId);
            }

            var existingBookings = await existingBookingsQuery
                .Select(b => new
                {
                    b.ID,
                    b.StylistID,
                    b.BookingDate,
                    b.ServiceDurationMinutesSnapshot,
                    b.RestTimeMinutesSnapshot,
                    b.Stylist.BookingCreationMode,
                    b.Stylist.SlotIntervalMinutes,
                    b.Stylist.RestTime
                })
                .ToListAsync();

            if (!existingBookings.Any())
                return false;

            var existingBookingIds = existingBookings.Select(x => x.ID).ToList();
            var existingServiceRows = await (
                from bs in _context.BookingServices.AsNoTracking()
                join b in _context.Bookings.AsNoTracking() on bs.BookingID equals b.ID
                join ss in _context.StylistServices.AsNoTracking()
                    on new { b.StylistID, bs.ServiceManagementID }
                    equals new { ss.StylistID, ss.ServiceManagementID }
                where existingBookingIds.Contains(bs.BookingID)
                select new
                {
                    bs.BookingID,
                    ss.ServiceDuration
                })
                .ToListAsync();

            var durationByBookingId = existingServiceRows
                .GroupBy(x => x.BookingID)
                .ToDictionary(
                    x => x.Key,
                    x => x.Sum(row => GetMinutes(row.ServiceDuration)));

            // تداخل دوطرفه: بازه‌ی نوبت جدید [newStart, newBlockEnd) با بازه‌ی هر نوبت موجود
            // [existingBooking.BookingDate, existingEnd) همپوشانی داشته باشد؛ کافی نیست فقط لحظه‌ی
            // شروع نوبت جدید داخل بازه‌ی نوبت موجود بیفتد، چون نوبت موجود هم می‌تواند وسط بازه‌ی نوبت جدید شروع شود.
            return existingBookings.Any(existingBooking =>
            {
                var existingIsManual = string.Equals(
                    existingBooking.BookingCreationMode,
                    "manual",
                    StringComparison.OrdinalIgnoreCase);
                var existingDuration = existingBooking.ServiceDurationMinutesSnapshot ??
                    (existingIsManual
                        ? NormalizeSlotIntervalMinutes(existingBooking.SlotIntervalMinutes)
                        : durationByBookingId.TryGetValue(existingBooking.ID, out var duration)
                            ? duration
                            : 0);

                var existingRest = existingBooking.RestTimeMinutesSnapshot ??
                    GetMinutes(existingBooking.RestTime);

                var existingEnd = existingBooking.BookingDate.AddMinutes(existingDuration + existingRest);
                return newStart < existingEnd && existingBooking.BookingDate < newBlockEnd;
            });
        }

        private async Task EnsureBookingIsInsideWorkTimeAsync(long stylistId, DateTime start, DateTime serviceEnd)
        {
            var workTimes = await _context.WorkTimes.AsNoTracking()
                .Where(x => x.StylistID == stylistId)
                .ToListAsync();

            var sameDayWorkTimes = workTimes
                .Where(x => MatchDayOfWeek(x.DayOfWeek, start.DayOfWeek))
                .ToList();

            if (!sameDayWorkTimes.Any())
                throw new InvalidOperationException("آرایشگر در روز انتخاب شده زمان کاری ندارد.");

            var startTime = start.TimeOfDay;
            var endTime = serviceEnd.TimeOfDay;

            var isInsideWorkTime = sameDayWorkTimes.Any(x =>
                x.WorkStartTime <= startTime &&
                x.WorkEndTime >= endTime);

            if (!isInsideWorkTime)
                throw new InvalidOperationException("زمان رزرو خارج از ساعت کاری آرایشگر است.");
        }

        private async Task EnsureBookingStartMatchesFixedIntervalAsync(
            long stylistId,
            DateTime start,
            int intervalMinutes,
            int restMinutes)
        {
            var stepMinutes = intervalMinutes + Math.Max(0, restMinutes);

            var workTimes = await _context.WorkTimes.AsNoTracking()
                .Where(x => x.StylistID == stylistId)
                .ToListAsync();

            var startTime = start.TimeOfDay;
            var matchingWindow = workTimes.FirstOrDefault(x =>
                MatchDayOfWeek(x.DayOfWeek, start.DayOfWeek) &&
                x.WorkStartTime <= startTime &&
                x.WorkEndTime > startTime);

            if (matchingWindow == null)
                return;

            var minutesFromWorkStart = (startTime - matchingWindow.WorkStartTime).TotalMinutes;
            var remainder = minutesFromWorkStart % stepMinutes;
            var isAligned = Math.Abs(remainder) < 0.001 ||
                            Math.Abs(remainder - stepMinutes) < 0.001;

            if (!isAligned)
            {
                throw new InvalidOperationException(
                    $"زمان شروع نوبت باید مطابق فاصله {stepMinutes} دقیقه‌ای " +
                    $"({intervalMinutes} دقیقه نوبت و {Math.Max(0, restMinutes)} دقیقه استراحت) باشد.");
            }
        }

        private async Task EnsureBookingIsOutsideStylistPacificAsync(long stylistId, DateTime start, DateTime end)
        {
            var hasPacificConflict = await _context.StylistPacifics.AsNoTracking()
                .AnyAsync(x =>
                    x.StylistID == stylistId &&
                    x.PacificStartDate < end &&
                    x.PacificEndDate > start);

            if (hasPacificConflict)
                throw new InvalidOperationException("آرایشگر در زمان انتخاب شده مرخصی دارد.");
        }

        public async Task<ListResultObject<BookingDTO>> MarkBookingsForRescheduleByLeaveAsync(
            long stylistId,
            DateTime start,
            DateTime end,
            string reason)
        {
            var result = new ListResultObject<BookingDTO>();
            try
            {
                var candidates = await _context.Bookings
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Where(x =>
                        x.StylistID == stylistId &&
                        !x.IsCancelled &&
                        x.Status == "1" &&
                        x.BookingDate < end)
                    .ToListAsync();

                var candidateIds = candidates.Select(x => x.ID).ToList();
                var durations = await (
                    from bs in _context.BookingServices.AsNoTracking()
                    join ss in _context.StylistServices.AsNoTracking()
                        on new { stylistId, bs.ServiceManagementID }
                        equals new { stylistId = ss.StylistID, ss.ServiceManagementID }
                    where candidateIds.Contains(bs.BookingID)
                    select new { bs.BookingID, ss.ServiceDuration })
                    .ToListAsync();

                var durationByBooking = durations
                    .GroupBy(x => x.BookingID)
                    .ToDictionary(x => x.Key, x => x.Sum(row => GetMinutes(row.ServiceDuration)));

                var affected = candidates
                    .Where(x =>
                    {
                        var duration = x.ServiceDurationMinutesSnapshot ??
                            (durationByBooking.TryGetValue(x.ID, out var minutes)
                                ? Math.Max(minutes, 15)
                                : 30);
                        return x.BookingDate.AddMinutes(duration) > start;
                    })
                    .ToList();

                foreach (var booking in affected)
                {
                    booking.Status = "5";
                    booking.UpdateDate = DateTime.Now.ToShamsi();
                }

                await _context.SaveChangesAsync();

                result.Results = affected.Select(booking =>
                {
                    var duration = booking.ServiceDurationMinutesSnapshot ??
                        (durationByBooking.TryGetValue(booking.ID, out var minutes)
                            ? Math.Max(minutes, 15)
                            : 30);
                    var restMinutes = booking.RestTimeMinutesSnapshot ?? 0;
                    return new BookingDTO
                    {
                        ID = booking.ID,
                        StylistID = booking.StylistID,
                        CustomerID = booking.CustomerID,
                        BookingStartDate = booking.BookingDate,
                        BookingEndDate = booking.BookingDate.AddMinutes(duration),
                        Status = booking.Status,
                        IsCancelled = booking.IsCancelled,
                        CancelReason = booking.CancelReason,
                        Description = booking.Description,
                        Stylist = booking.Stylist,
                        Customer = booking.Customer,
                        TotalDurationMinutes = duration,
                        TotalBlockMinutes = duration + restMinutes
                        ,
                        ServiceIDs = _context.BookingServices
                            .Where(bs => bs.BookingID == booking.ID)
                            .Select(bs => bs.ServiceManagementID)
                            .ToList()
                    };
                }).ToList();
                result.TotalCount = result.Results.Count;
                result.PageCount = result.TotalCount > 0 ? 1 : 0;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<ListResultObject<BookingDTO>> RestoreBookingsAfterLeaveDeleteAsync(
            long stylistId,
            DateTime start,
            DateTime end)
        {
            var result = new ListResultObject<BookingDTO>();
            try
            {
                // نوبت‌های Status==5 که در بازه این مرخصی قرار دارند
                var candidates = await _context.Bookings
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.Stylist)
                    .Where(x =>
                        x.StylistID == stylistId &&
                        !x.IsCancelled &&
                        x.Status == "5" &&
                        x.BookingDate >= start &&
                        x.BookingDate < end)
                    .ToListAsync();

                var restored = new List<Booking>();
                foreach (var booking in candidates)
                {
                    var bookingBlockMinutes =
                        (booking.ServiceDurationMinutesSnapshot ?? 30) +
                        (booking.RestTimeMinutesSnapshot ?? 0);
                    // اگر هیچ مرخصی فعال دیگری این نوبت را پوشش نمی‌دهد، برگردانیم به Status 1
                    var stillCoveredByOtherLeave = await _context.StylistPacifics
                        .AsNoTracking()
                        .AnyAsync(p =>
                            p.StylistID == stylistId &&
                            p.PacificStartDate < booking.BookingDate.AddMinutes(bookingBlockMinutes) &&
                            p.PacificEndDate > booking.BookingDate);

                    if (!stillCoveredByOtherLeave)
                    {
                        booking.Status = "1";
                        booking.UpdateDate = DateTime.Now.ToShamsi();
                        restored.Add(booking);
                    }
                }

                await _context.SaveChangesAsync();

                result.Results = restored.Select(booking => new BookingDTO
                {
                    ID = booking.ID,
                    StylistID = booking.StylistID,
                    CustomerID = booking.CustomerID,
                    BookingStartDate = booking.BookingDate,
                    BookingEndDate = booking.BookingDate.AddMinutes(
                        booking.ServiceDurationMinutesSnapshot ?? 30),
                    Status = booking.Status,
                    Stylist = booking.Stylist,
                    Customer = booking.Customer,
                }).ToList();
                result.TotalCount = result.Results.Count;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        private static int GetMinutes(TimeSpan? time)
        {
            return time == null ? 0 : Convert.ToInt32(time.Value.TotalMinutes);
        }

        private static int GetMinutes(TimeSpan time)
        {
            return Convert.ToInt32(time.TotalMinutes);
        }

        private static int NormalizeSlotIntervalMinutes(int configuredIntervalMinutes)
        {
            return configuredIntervalMinutes is >= 5 and <= 240
                ? configuredIntervalMinutes
                : 30;
        }

        private static bool MatchDayOfWeek(string dayName, DayOfWeek dayOfWeek)
        {
            var normalized = (dayName ?? string.Empty)
                .Replace("ي", "ی")
                .Replace("ك", "ک")
                .Replace("‌", "")
                .Trim();

            return dayOfWeek switch
            {
                DayOfWeek.Saturday => normalized.Contains("شنبه") && !normalized.Contains("یک") && !normalized.Contains("دو") && !normalized.Contains("سه") && !normalized.Contains("چهار") && !normalized.Contains("پنج"),
                DayOfWeek.Sunday => normalized.Contains("یکشنبه"),
                DayOfWeek.Monday => normalized.Contains("دوشنبه"),
                DayOfWeek.Tuesday => normalized.Contains("سهشنبه") || normalized.Contains("سه"),
                DayOfWeek.Wednesday => normalized.Contains("چهارشنبه") || normalized.Contains("چهار"),
                DayOfWeek.Thursday => normalized.Contains("پنجشنبه") || normalized.Contains("پنج"),
                DayOfWeek.Friday => normalized.Contains("جمعه"),
                _ => false
            };
        }
    }
}
