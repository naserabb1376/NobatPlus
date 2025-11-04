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
    int cancelState = 0,
    int pageIndex = 1,
    int pageSize = 20,
    string searchText = "",
    string sortQuery = "") // 🔹 پارامتر جدید برای فیلتر سرویس
        {
            ListResultObject<BookingDTO> results = new ListResultObject<BookingDTO>();

            try
            {
                IQueryable<Booking> bookingsQuery;

                // 🔹 اگر ServiceManagementId مقدار دارد، از BookingServices بگیر
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

                // 🔹 فیلتر وضعیت لغو
                if (cancelState == 1)
                    bookingsQuery = bookingsQuery.Where(x => x.IsCancelled);
                else if (cancelState == 2)
                    bookingsQuery = bookingsQuery.Where(x => !x.IsCancelled);

                // 🔹 جستجو در فیلدها
                if (!string.IsNullOrEmpty(searchText))
                {
                    bookingsQuery = bookingsQuery.Where(x =>
                        x.Stylist.Person.FirstName.Contains(searchText) ||
                        x.Stylist.Person.LastName.Contains(searchText) ||
                        x.Description.Contains(searchText) ||
                        x.Status.ToString().Contains(searchText));
                }

                // 🔹 لود ارتباطات اصلی
                bookingsQuery = bookingsQuery
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .AsNoTracking();

                // 🔹 زیرکوئری محاسبه مجموع زمان‌ها
                var stylistDurations = _context.StylistServices
                    .GroupBy(s => s.StylistID)
                    .Select(g => new
                    {
                        StylistID = g.Key,
                        TotalSeconds = (int?)g.Sum(x => EF.Functions.DateDiffSecond(TimeSpan.Zero, x.ServiceDuration))
                    });

                // 🔹 صفحه‌بندی و خروجی
                results.TotalCount = await bookingsQuery.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await (from b in bookingsQuery
                                         join d in stylistDurations on b.StylistID equals d.StylistID into gj
                                         from d in gj.DefaultIfEmpty()
                                         orderby b.CreateDate descending
                                         select new BookingDTO
                                         {
                                             ID = b.ID,
                                             StylistID = b.StylistID,
                                             CustomerID = b.CustomerID,
                                             Description = b.Description,
                                             BookingDate = b.BookingDate,
                                             BookingTime = b.BookingTime,
                                             CancelReason = b.CancelReason,
                                             IsCancelled = b.IsCancelled,
                                             Status = b.Status,
                                             Stylist = b.Stylist,
                                             Customer = b.Customer,
                                             UpdateDate = b.UpdateDate,
                                             CreateDate = b.CreateDate,
                                             TotalTimeDuration = TimeSpan.FromSeconds(d.TotalSeconds.HasValue ? d.TotalSeconds.Value : 0)
                                         })
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
            RowResultObject<BookingDTO> result = new RowResultObject<BookingDTO>();

            try
            {
                var bookingQuery = _context.Bookings
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .AsNoTracking()
                    .Where(x => x.ID == bookingId);

                var stylistDurations = _context.StylistServices
                    .GroupBy(s => s.StylistID)
                    .Select(g => new
                    {
                        StylistID = g.Key,
                        TotalSeconds = (int?)g.Sum(x => EF.Functions.DateDiffSecond(TimeSpan.Zero, x.ServiceDuration))
                    });

                result.Result = await (from b in bookingQuery
                                       join d in stylistDurations on b.StylistID equals d.StylistID into gj
                                       from d in gj.DefaultIfEmpty()
                                       select new BookingDTO
                                       {
                                           ID = b.ID,
                                           StylistID = b.StylistID,
                                           CustomerID = b.CustomerID,
                                           Description = b.Description,
                                           BookingDate = b.BookingDate,
                                           BookingTime = b.BookingTime,
                                           CancelReason = b.CancelReason,
                                           IsCancelled = b.IsCancelled,
                                           Status = b.Status,
                                           Stylist = b.Stylist,
                                           Customer = b.Customer,
                                           UpdateDate = b.UpdateDate,
                                           CreateDate = b.CreateDate,
                                           TotalTimeDuration = TimeSpan.FromSeconds(d.TotalSeconds.Value)
                                       })
                                      .SingleOrDefaultAsync();
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
                    BookingDate = BookingDto.Result.BookingDate,
                    BookingTime = BookingDto.Result.BookingTime,
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
    }
}
