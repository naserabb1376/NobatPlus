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
                var bookingServiceIds = Booking.BookingServices.Select(x => x.ServiceManagementID).ToList();

                bool hasConfilict = await HasBookingConflictForStylistOrCustomerAsync(Booking.StylistID, Booking.CustomerID, Booking.BookingDate, bookingServiceIds,Booking.ID);

                if (hasConfilict)
                {
                    throw new Exception("ثبت این نوبت به دلیل وجود تداخل برای مشتری / آرایشگر امکان پذیر نیست");
                }


                _context.Bookings.Update(Booking);
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
                        .Select(bs => bs.Booking);
                }
                else
                {
                    bookingsQuery = _context.Bookings;
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
                    bookingsQuery = bookingsQuery.Where(x=> x.BookingDate >= fromDate.Value.ToShamsi());
                }

                if (toDate != null)
                {
                    bookingsQuery = bookingsQuery.Where(x => x.BookingDate <= toDate.Value.ToShamsi());
                }

                if (!string.IsNullOrEmpty(searchText))
                {
                    bookingsQuery = bookingsQuery.Where(x =>
                        x.Stylist.Person.FirstName.Contains(searchText) ||
                        x.Stylist.Person.LastName.Contains(searchText) ||
                        x.Status.Contains(searchText));
                }

                bookingsQuery = bookingsQuery
                    .Include(x => x.Stylist).ThenInclude(x=> x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .AsNoTracking();

                IQueryable<BookingDurationRow> bookingDurations;

                try
                {
                    bookingDurations =
                        from bs in _context.BookingServices
                        join b in _context.Bookings on bs.BookingID equals b.ID
                        join ss in _context.StylistServices
                            on new { b.StylistID, bs.ServiceManagementID }
                            equals new { ss.StylistID, ss.ServiceManagementID }
                        group ss by bs.BookingID into g
                        select new BookingDurationRow
                        {
                            BookingID = g.Key,
                            TotalDurationMinutes = g.Sum(x =>
                                EF.Functions.DateDiffMinute(TimeSpan.Zero, x.ServiceDuration)
                            )
                        };
                }
                catch
                {
                    bookingDurations =
                        Enumerable.Empty<BookingDurationRow>()
                                  .AsQueryable();
                }


                results.TotalCount = await bookingsQuery.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await (
     from b in bookingsQuery
     join d in bookingDurations on b.ID equals d.BookingID into gj
     from d in gj.DefaultIfEmpty()
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

         TotalDurationMinutes = d == null ? 0 : d.TotalDurationMinutes,

         BookingEndDate = b.BookingDate.AddMinutes(d == null ? 0 : d.TotalDurationMinutes),

         TotalBlockMinutes =
             (d == null ? 0 : d.TotalDurationMinutes) +
             EF.Functions.DateDiffMinute(TimeSpan.Zero, b.Stylist.RestTime),

         Status = b.Status,
         IsCancelled = b.IsCancelled,
         CancelReason = b.CancelReason,

         Stylist = b.Stylist,
         Customer = b.Customer
     }
 )
 .SortBy(sortQuery)
 .ToPaging(pageIndex, pageSize)
 .ToListAsync();


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

                // -------- Safe bookingDurations --------
                IQueryable<BookingDurationRow> bookingDurations;

                try
                {
                    bookingDurations =
                        from bs in _context.BookingServices
                        join b in _context.Bookings on bs.BookingID equals b.ID
                        join ss in _context.StylistServices
                            on new { b.StylistID, bs.ServiceManagementID }
                            equals new { ss.StylistID, ss.ServiceManagementID }
                        group ss by bs.BookingID into g
                        select new BookingDurationRow
                        {
                            BookingID = g.Key,
                            TotalDurationMinutes = g.Sum(x =>
                                x.ServiceDuration == null
                                    ? 0
                                    : EF.Functions.DateDiffMinute(
                                        TimeSpan.Zero,
                                        x.ServiceDuration
                                    )
                            )
                        };
                }
                catch
                {
                    bookingDurations =
                        Enumerable.Empty<BookingDurationRow>().AsQueryable();
                }

                // -------- Main Query --------
                result.Result = await (
                    from b in bookingQuery
                    join d in bookingDurations
                        on b.ID equals d.BookingID into gj
                    from d in gj.DefaultIfEmpty()
                    select new BookingDTO
                    {
                        ID = b.ID,
                        StylistID = b.StylistID,
                        CustomerID = b.CustomerID,

                        CreateDate = b.CreateDate,
                        UpdateDate = b.UpdateDate,
                        Description = b.Description,

                        BookingStartDate = b.BookingDate,

                        TotalDurationMinutes = d == null ? 0 : d.TotalDurationMinutes,

                        BookingEndDate = b.BookingDate.AddMinutes(
                            d == null ? 0 : d.TotalDurationMinutes
                        ),

                        TotalBlockMinutes =
                            (d == null ? 0 : d.TotalDurationMinutes) +
                            EF.Functions.DateDiffMinute(
                                TimeSpan.Zero,
                                b.Stylist.RestTime
                            ),

                        Status = b.Status,
                        IsCancelled = b.IsCancelled,
                        CancelReason = b.CancelReason,

                        Stylist = b.Stylist,
                        Customer = b.Customer
                    }
                ).SingleOrDefaultAsync();
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
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

            // -------- 1) RestTime minutes for stylist --------
            var restMinutes = await _context.Stylists.AsNoTracking()
                .Where(s => s.ID == stylistId)
                .Select(s => EF.Functions.DateDiffMinute(TimeSpan.Zero, s.RestTime))
                .SingleOrDefaultAsync();

            // -------- 2) NEW booking duration minutes (selected services for this stylist) --------
            var newDurationMinutes = await _context.StylistServices.AsNoTracking()
                .Where(ss => ss.StylistID == stylistId && serviceManagementIds.Contains(ss.ServiceManagementID))
                .Select(ss => ss.ServiceDuration == null
                    ? 0
                    : EF.Functions.DateDiffMinute(TimeSpan.Zero, ss.ServiceDuration))
                .SumAsync();

            // block time = duration + rest
            var newBlockMinutes = newDurationMinutes + restMinutes;
            var newEnd = newStart.AddMinutes(newBlockMinutes);

            // -------- 3) Durations for EXISTING bookings (same algo as your GetBookingByIdAsync) --------
            IQueryable<BookingDurationRow> bookingDurations =
                from bs in _context.BookingServices.AsNoTracking()
                join b in _context.Bookings.AsNoTracking() on bs.BookingID equals b.ID
                join ss in _context.StylistServices.AsNoTracking()
                    on new { b.StylistID, bs.ServiceManagementID }
                    equals new { ss.StylistID, ss.ServiceManagementID }
                group ss by bs.BookingID into g
                select new BookingDurationRow
                {
                    BookingID = g.Key,
                    TotalDurationMinutes = g.Sum(x =>
                        x.ServiceDuration == null
                            ? 0
                            : EF.Functions.DateDiffMinute(TimeSpan.Zero, x.ServiceDuration)
                    )
                };

            // -------- 4) Base query: bookings that belong to stylist OR customer, and are active --------
            var existingBookings = _context.Bookings.AsNoTracking()
                .Where(b => (b.StylistID == stylistId) || (b.CustomerID == customerId))
                .Where(b => !b.IsCancelled);

            if (bookingId > 0)
            {
                existingBookings = existingBookings.Where(b=> b.ID != bookingId);
            }

            // -------- 5) Overlap check (SQL-side) --------
            var conflictQuery =
                from b in existingBookings
                join d in bookingDurations on b.ID equals d.BookingID into gj
                from d in gj.DefaultIfEmpty()
                let existingDuration = (d == null ? 0 : d.TotalDurationMinutes)
                let existingBlockMinutes = existingDuration + restMinutes
                let existingStart = b.BookingDate
                let existingEnd = b.BookingDate.AddMinutes(existingBlockMinutes)
                where existingStart < newEnd && existingEnd > newStart
                select b.ID;

            return await conflictQuery.AnyAsync();
        }
    }
}
