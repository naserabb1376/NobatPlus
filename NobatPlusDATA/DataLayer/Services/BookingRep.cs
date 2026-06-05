using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
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
                var bookingServiceIds = Booking.BookingServices.Select(x => x.ServiceManagementID).ToList();

                bool hasConfilict = await HasBookingConflictForStylistOrCustomerAsync(Booking.StylistID,Booking.CustomerID,Booking.BookingDate,bookingServiceIds);

                if (hasConfilict)
                {
                    throw new Exception("ثبت این نوبت به دلیل وجود تداخل برای مشتری / آرایشگر امکان پذیر نیست");
                }

                await _context.Bookings.AddAsync(Booking);
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

        public async Task<BitResultObject> EditBookingAsync(Booking Booking)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var previousBooking = await _context.Bookings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ID == Booking.ID);

                if (previousBooking == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "رزرو یافت نشد";
                    return result;
                }

                var bookingServiceIds = Booking.BookingServices.Select(x => x.ServiceManagementID).ToList();

                bool hasConfilict = await HasBookingConflictForStylistOrCustomerAsync(Booking.StylistID, Booking.CustomerID, Booking.BookingDate, bookingServiceIds,Booking.ID);

                if (hasConfilict)
                {
                    throw new Exception("ثبت این نوبت به دلیل وجود تداخل برای مشتری / آرایشگر امکان پذیر نیست");
                }

                foreach (var bookingService in Booking.BookingServices ?? new List<BookingService>())
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

                _context.Bookings.Update(Booking);
                if (!previousBooking.IsCancelled && Booking.IsCancelled)
                {
                    await RefundWalletPaymentsForCancelledBookingAsync(Booking.ID);
                }

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

        private async Task RefundWalletPaymentsForCancelledBookingAsync(long bookingId)
        {
            var now = DateTime.Now.ToShamsi();
            var walletPayments = await _context.WalletTransactions
                .Include(x => x.Wallet)
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
            string sortQuery = "")
        {
            ListResultObject<BookingDTO> results = new();

            try
            {
                IQueryable<Booking> bookingsQuery;

                if (serviceManagementId > 0)
                {
                    bookingsQuery = _context.BookingServices
                        .Where(bs => bs.ServiceManagementID == serviceManagementId)
                        .Select(bs => bs.Booking)
                        .AsNoTracking();
                }
                else
                {
                    bookingsQuery = _context.Bookings.AsNoTracking();
                }

                if (customerId > 0)
                    bookingsQuery = bookingsQuery.Where(x => x.CustomerID == customerId);

                if (stylistId > 0)
                    bookingsQuery = bookingsQuery.Where(x => x.StylistID == stylistId);

                if (cancelState == 1)
                    bookingsQuery = bookingsQuery.Where(x => x.IsCancelled);
                else if (cancelState == 2)
                    bookingsQuery = bookingsQuery.Where(x => !x.IsCancelled);

                if (fromDate != null)
                {
                    var from = fromDate.Value.ToShamsi();
                    bookingsQuery = bookingsQuery.Where(x => x.BookingDate >= from);
                }

                if (toDate != null)
                {
                    var to = toDate.Value.ToShamsi();
                    bookingsQuery = bookingsQuery.Where(x => x.BookingDate <= to);
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    bookingsQuery = bookingsQuery.Where(x =>
                        x.Stylist.Person.FirstName.Contains(searchText) ||
                        x.Stylist.Person.LastName.Contains(searchText) ||
                        x.Status.Contains(searchText));
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

                        TotalDurationMinutes = totalDurationMinutes,

                        BookingEndDate = b.BookingDate.AddMinutes(totalDurationMinutes),

                        TotalBlockMinutes = totalDurationMinutes + restMinutes,

                        Status = b.Status,
                        IsCancelled = b.IsCancelled,
                        CancelReason = b.CancelReason,

                        Stylist = b.Stylist,
                        Customer = b.Customer,
                        Services = b.BookingServices.Select(bs => new BookingServiceSelectionDTO
                        {
                            ServiceID = bs.ServiceManagementID,
                            ServiceName = bs.ServiceManagement.ServiceName,
                            OptionValueIDs = bs.OptionValues.Select(ov => ov.ServiceOptionValueID).ToList(),
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
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
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

                    select new BookingDTO
                    {
                        ID = b.ID,
                        StylistID = b.StylistID,
                        CustomerID = b.CustomerID,

                        CreateDate = b.CreateDate,
                        UpdateDate = b.UpdateDate,
                        Description = b.Description,

                        BookingStartDate = b.BookingDate,

                        TotalDurationMinutes = totalDurationMinutes,

                        BookingEndDate = b.BookingDate.AddMinutes(totalDurationMinutes),

                        TotalBlockMinutes = totalDurationMinutes + restMinutes,

                        Status = b.Status,
                        IsCancelled = b.IsCancelled,
                        CancelReason = b.CancelReason,

                        Stylist = b.Stylist,
                        Customer = b.Customer,
                        Services = b.BookingServices.Select(bs => new BookingServiceSelectionDTO
                        {
                            ServiceID = bs.ServiceManagementID,
                            ServiceName = bs.ServiceManagement.ServiceName,
                            OptionValueIDs = bs.OptionValues.Select(ov => ov.ServiceOptionValueID).ToList(),
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

        public async Task<bool> HasBookingConflictForStylistOrCustomerAsync(
    long stylistId,
    long customerId,
    DateTime newStart,
    List<long> serviceManagementIds,
    long bookingId = 0)
        {
            if (serviceManagementIds == null || serviceManagementIds.Count == 0)
                throw new ArgumentException("حداقل یک سرویس باید انتخاب شود.", nameof(serviceManagementIds));

            serviceManagementIds = serviceManagementIds
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            var stylist = await _context.Stylists.AsNoTracking()
                .Where(s => s.ID == stylistId)
                .Select(s => new { s.ID, s.RestTime })
                .SingleOrDefaultAsync();

            if (stylist == null)
                throw new ArgumentException("آرایشگر یافت نشد.", nameof(stylistId));

            var restMinutes = GetMinutes(stylist.RestTime);

            var selectedServiceDurations = await _context.StylistServices.AsNoTracking()
                .Where(ss => ss.StylistID == stylistId && serviceManagementIds.Contains(ss.ServiceManagementID))
                .Select(ss => ss.ServiceDuration)
                .ToListAsync();

            if (selectedServiceDurations.Count != serviceManagementIds.Count)
                throw new ArgumentException("یک یا چند سرویس برای این آرایشگر تعریف نشده است.", nameof(serviceManagementIds));

            var newDurationMinutes = selectedServiceDurations.Sum(GetMinutes);
            var newServiceEnd = newStart.AddMinutes(newDurationMinutes);
            var newBlockEnd = newServiceEnd.AddMinutes(restMinutes);

            await EnsureBookingIsInsideWorkTimeAsync(stylistId, newStart, newServiceEnd);
            await EnsureBookingIsOutsideStylistPacificAsync(stylistId, newStart, newBlockEnd);

            var existingBookingsQuery = _context.Bookings.AsNoTracking()
                .Where(b => (b.StylistID == stylistId) || (b.CustomerID == customerId))
                .Where(b => !b.IsCancelled);

            if (bookingId > 0)
            {
                existingBookingsQuery = existingBookingsQuery.Where(b => b.ID != bookingId);
            }

            var existingBookings = await existingBookingsQuery
                .Select(b => new { b.ID, b.StylistID, b.BookingDate })
                .ToListAsync();

            if (!existingBookings.Any())
                return false;

            var existingBookingIds = existingBookings.Select(x => x.ID).ToList();
            var existingStylistIds = existingBookings.Select(x => x.StylistID).Distinct().ToList();

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

            var restByStylistId = await _context.Stylists.AsNoTracking()
                .Where(s => existingStylistIds.Contains(s.ID))
                .Select(s => new { s.ID, s.RestTime })
                .ToDictionaryAsync(x => x.ID, x => GetMinutes(x.RestTime));

            return existingBookings.Any(existingBooking =>
            {
                var existingDuration = durationByBookingId.TryGetValue(existingBooking.ID, out var duration)
                    ? duration
                    : 0;

                var existingRest = restByStylistId.TryGetValue(existingBooking.StylistID, out var rest)
                    ? rest
                    : 0;

                var existingEnd = existingBooking.BookingDate.AddMinutes(existingDuration + existingRest);
                return existingBooking.BookingDate < newBlockEnd && existingEnd > newStart;
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

        private static int GetMinutes(TimeSpan? time)
        {
            return time == null ? 0 : Convert.ToInt32(time.Value.TotalMinutes);
        }

        private static int GetMinutes(TimeSpan time)
        {
            return Convert.ToInt32(time.TotalMinutes);
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
