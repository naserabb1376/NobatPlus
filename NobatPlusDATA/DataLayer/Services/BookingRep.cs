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
        public BookingRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddBookingAsync(Booking Booking)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Bookings.Add(Booking);
                await _context.SaveChangesAsync();
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
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
             
        }

        public async Task<ListResultObject<Booking>> GetAllBookingsAsync(int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Booking> results = new ListResultObject<Booking>();
            try
            {
                IQueryable<Booking> query;

                if (cancelState == 0)
                {
                    query = _context.Bookings
                        .AsNoTracking()
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Include(x => x.Customer).ThenInclude(x => x.Person)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        );
                }
                else if (cancelState == 1)
                {
                    query = _context.Bookings
                        .AsNoTracking()
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Include(x => x.Customer).ThenInclude(x => x.Person)
                        .Where(x => (x.IsCancelled) &&
                            ((!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        ));
                }
                else
                {
                    query = _context.Bookings
                        .AsNoTracking()
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Include(x => x.Customer).ThenInclude(x => x.Person)
                        .Where(x => (!x.IsCancelled) &&
                            ((!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        ));
                        
                    
                }
                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
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

        public async Task<ListResultObject<Booking>> GetBookingsOfServiceAsync(int ServiceManagementId, int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Booking> results = new ListResultObject<Booking>();
            try
            {
                IQueryable<Booking> query;

                if (cancelState == 0)
                {
                    query = _context.BookingServices
                    .Where(bs => bs.ServiceManagementID == ServiceManagementId)
                    .Select(bs => bs.Booking)
                    .AsNoTracking()
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Where(x =>
                        (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    );
                }
                else if (cancelState == 1)
                {
                    query = _context.BookingServices
                    .Where(bs => bs.ServiceManagementID == ServiceManagementId)
                    .Select(bs => bs.Booking)
                    .AsNoTracking()
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Where(x => (x.IsCancelled) &&
                        ((!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    ));
                }
                else
                {
                    query = _context.BookingServices
                    .Where(bs => bs.ServiceManagementID == ServiceManagementId)
                    .Select(bs => bs.Booking)
                    .AsNoTracking()
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Where(x => (!x.IsCancelled) &&
                        ((!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                        || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    ));
               }
                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
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

        public async Task<RowResultObject<Booking>> GetBookingByIdAsync(long BookingId)
        {
            RowResultObject<Booking> result = new RowResultObject<Booking>();
            try
            {
                result.Result = await _context.Bookings
                .AsNoTracking()
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.Customer).ThenInclude(x => x.Person)
                .SingleOrDefaultAsync(x => x.ID == BookingId);
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
                var Booking = await GetBookingByIdAsync(BookingId);
                result = await RemoveBookingAsync(Booking.Result);
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
