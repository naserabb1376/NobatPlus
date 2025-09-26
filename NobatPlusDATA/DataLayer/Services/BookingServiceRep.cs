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
    public class BookingServiceRep : IBookingServiceRep
    {

        private NobatPlusContext _context;
        public BookingServiceRep(NobatPlusContext context)
        {
            _context = context;
        }


        public async Task<BitResultObject> AddBookingServicesAsync(List<BookingService> bookingServices)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.BookingServices.AddRangeAsync(bookingServices);
                await _context.SaveChangesAsync();
                result.ID = bookingServices.FirstOrDefault().BookingID;
                foreach (var bookingService in bookingServices)
                {
                    _context.Entry(bookingService).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditBookingServicesAsync(List<BookingService> bookingServices)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.BookingServices.UpdateRange(bookingServices);
                await _context.SaveChangesAsync();
                result.ID = bookingServices.FirstOrDefault().BookingID;
                foreach (var bookingService in bookingServices)
                {
                    _context.Entry(bookingService).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveBookingServicesAsync(List<BookingService> bookingServices)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.BookingServices.RemoveRange(bookingServices);
                await _context.SaveChangesAsync();
                result.ID = bookingServices.FirstOrDefault().BookingID;
                foreach (var bookingService in bookingServices)
                {
                    _context.Entry(bookingService).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> RemoveBookingServicesAsync(List<(long BookingId, long ServiceManagementId)> bookingServiceIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var bookingServicesToRemove = new List<BookingService>();

                foreach (var (bookingId, serviceManagementId) in bookingServiceIds)
                {
                    var bookingService = await GetBookingServiceByIdAsync(bookingId, serviceManagementId);
                    if (bookingService.Result != null)
                    {
                        bookingServicesToRemove.Add(bookingService.Result);
                    }
                }

                if (bookingServicesToRemove.Any())
                {
                    result = await RemoveBookingServicesAsync(bookingServicesToRemove);
                }
                else
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching booking services found to remove.";
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> ExistBookingServiceAsync(long BookingId, long ServiceManagementId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status= await _context.BookingServices
                .AsNoTracking()
                .AnyAsync(x => x.BookingID == BookingId && x.ServiceManagementID == ServiceManagementId);
                result.ID = BookingId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
  
        }

        public async Task<ListResultObject<BookingService>> GetAllBookingServicesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<BookingService> results = new ListResultObject<BookingService>();
            try
            {
                var query = _context.BookingServices
                .AsNoTracking()
                .Include(x => x.Booking).ThenInclude(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.Booking).ThenInclude(x => x.Customer).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement)
                .Where(x =>
                    (!string.IsNullOrEmpty(x.Booking.BookingDate.ToString()) && x.Booking.BookingDate.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Booking.BookingTime.ToString()) && x.Booking.BookingTime.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Booking.Status.ToString()) && x.Booking.Status.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName.ToString()) && x.ServiceManagement.ServiceName.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Booking.Stylist.Specialty.ToString()) && x.Booking.Stylist.Specialty.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Booking.Stylist.Person.FirstName.ToString()) && x.Booking.Stylist.Person.FirstName.ToString().Contains(searchText))
                    || (!string.IsNullOrEmpty(x.Booking.Stylist.Person.LastName.ToString()) && x.Booking.Stylist.Person.LastName.ToString().Contains(searchText))
                );
                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.Booking.BookingDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
           
        }

        public async Task<RowResultObject<BookingService>> GetBookingServiceByIdAsync(long BookingId, long ServiceManagementId)
        {
            RowResultObject<BookingService> result = new RowResultObject<BookingService>();
            try
            {
                result.Result = await _context.BookingServices
                .AsNoTracking()
                .Include(x => x.Booking).ThenInclude(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.Booking).ThenInclude(x => x.Customer).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement)
                .SingleOrDefaultAsync(x => x.BookingID == BookingId && x.ServiceManagementID == ServiceManagementId);
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
