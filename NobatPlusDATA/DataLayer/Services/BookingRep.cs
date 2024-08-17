using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
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

        public async Task AddBookingAsync(Booking Booking)
        {
            _context.Bookings.Add(Booking);
            await _context.SaveChangesAsync();
            _context.Entry(Booking).State = EntityState.Detached;
        }

        public async Task EditBookingAsync(Booking Booking)
        {
            _context.Bookings.Update(Booking);
            await _context.SaveChangesAsync();
            _context.Entry(Booking).State = EntityState.Detached;
        }

        public async Task<bool> ExistBookingAsync(long BookingId)
        {
            return await _context.Bookings.AsNoTracking().AnyAsync(x => x.ID == BookingId);
        }

        public async Task<List<Booking>> GetAllBookingsAsync(int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (cancelState == 0)
            {
                return await _context.Bookings
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
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            else if (cancelState == 1)
            {
                return await _context.Bookings
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
                    ))
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            else
            {
                return await _context.Bookings
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
                    ))
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
        }

        public async Task<List<Booking>> GetBookingsOfServiceAsync(int ServiceManagementId, int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (cancelState == 0)
            {
                return await _context.BookingServices
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
                    )
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            else if (cancelState == 1)
            {
                return await _context.BookingServices
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
                    ))
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            else
            {
                return await _context.BookingServices
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
                    ))
                    .OrderByDescending(x => x.CreateDate)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
        }

        public async Task<Booking> GetBookingByIdAsync(long BookingId)
        {
            return await _context.Bookings
                .AsNoTracking()
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.Customer).ThenInclude(x => x.Person)
                .SingleOrDefaultAsync(x => x.ID == BookingId);
        }

        public async Task RemoveBookingAsync(Booking Booking)
        {
            _context.Bookings.Remove(Booking);
            await _context.SaveChangesAsync();
            _context.Entry(Booking).State = EntityState.Detached;
        }

        public async Task RemoveBookingAsync(long BookingId)
        {
            var Booking = await _context.Bookings.FirstOrDefaultAsync(x => x.ID == BookingId);
            _context.Bookings.Remove(Booking);
            await _context.SaveChangesAsync();
            _context.Entry(Booking).State = EntityState.Detached;
        }
    }
}
