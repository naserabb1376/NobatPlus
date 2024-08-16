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
    public class BookingServiceRep : IBookingServiceRep
    {

        private NobatPlusContext _context;
        public BookingServiceRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddBookingService(BookingService BookingService)
        {
            _context.BookingServices.Add(BookingService);
            _context.SaveChanges();
            _context.Entry(BookingService).State = EntityState.Detached;
        }

        public void EditBookingService(BookingService BookingService)
        {
            _context.BookingServices.Update(BookingService);
            _context.SaveChanges();
            _context.Entry(BookingService).State = EntityState.Detached;
        }

        public bool ExistBookingService(long BookingId, long ServiceManagementId)
        {
            return _context.BookingServices.Any(x => x.BookingID == BookingId && x.ServiceManagementID == ServiceManagementId);
        }

        public List<BookingService> GetAllBookingServices(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.BookingServices.Include(x=> x.Booking).ThenInclude(x=> x.Stylist).ThenInclude(x=> x.Person).Include(x=> x.ServiceManagement).Where(x =>
            (!string.IsNullOrEmpty(x.Booking.BookingDate.ToString()) && x.Booking.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Booking.BookingTime.ToString()) && x.Booking.BookingTime.ToString("HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Booking.Status.ToString()) && x.Booking.Status.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName.ToString()) && x.ServiceManagement.ServiceName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Booking.Stylist.Specialty.ToString()) && x.Booking.Stylist.Specialty.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Booking.Stylist.Person.FirstName.ToString()) && x.Booking.Stylist.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Booking.Stylist.Person.LastName.ToString()) && x.Booking.Stylist.Person.LastName.ToString().Contains(searchText))
            ).OrderByDescending(x => x.Booking.BookingDate).ToPaging(pageIndex,pageSize).ToList();
        }

        public BookingService GetBookingServiceById(long BookingId, long ServiceManagementId)
        {
            return _context.BookingServices.Include(x => x.Booking).ThenInclude(x => x.Stylist).ThenInclude(x => x.Person).Include(x => x.ServiceManagement).SingleOrDefault(x => x.BookingID == BookingId && x.ServiceManagementID == ServiceManagementId);
        }

        public void RemoveBookingService(BookingService BookingService)
        {
            _context.BookingServices.Remove(BookingService);
            _context.SaveChanges();
            _context.Entry(BookingService).State = EntityState.Detached;
        }

        public void RemoveBookingService(long BookingId, long ServiceManagementId)
        {
            var BookingService = GetBookingServiceById(BookingId,ServiceManagementId);
            RemoveBookingService(BookingService);
        }
    }
}
