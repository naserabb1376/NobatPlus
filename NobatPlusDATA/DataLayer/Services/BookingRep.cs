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

        public void AddBooking(Booking Booking)
        {
            _context.Bookings.Add(Booking);
            _context.SaveChanges();
            _context.Entry(Booking).State = EntityState.Detached;
        }

        public void EditBooking(Booking Booking)
        {
            _context.Bookings.Update(Booking);
            _context.SaveChanges();
            _context.Entry(Booking).State = EntityState.Detached;
        }

        public bool ExistBooking(long BookingId)
        {
            return _context.Bookings.Any(x => x.ID == BookingId);
        }

        public List<Booking> GetAllBookings(int cancelState = 0, int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            if (cancelState == 0)
            {
                return _context.Bookings.Include(x => x.Stylist).ThenInclude(x=> x.Person).Include(x => x.Customer).ThenInclude(x=> x.Person).Where(x =>
        (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
       || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
       || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
       || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
       || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
       || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
       || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
       || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
       || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
        ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else if(cancelState == 1)
            {

                return _context.Bookings.Include(x => x.Stylist).ThenInclude(x => x.Person).Include(x => x.Customer).ThenInclude(x => x.Person)
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
        )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.Bookings.Include(x => x.Stylist).ThenInclude(x => x.Person).Include(x => x.Customer).ThenInclude(x => x.Person)
                   .Where(x => (!x.IsCancelled) &&
       ((!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
      || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
      || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
      || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
       )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public List<Booking> GetBookingsOfService(int ServiceManagementId, int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (cancelState == 0)
            {
                return _context.BookingServices
           .Where(bs => bs.ServiceManagementID == ServiceManagementId)
           .Select(bs => bs.Booking).Include(x => x.Stylist).ThenInclude(x => x.Person).Include(x => x.Customer).ThenInclude(x => x.Person)
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
        ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else if (cancelState == 1)
            {
                return _context.BookingServices
             .Where(bs => bs.ServiceManagementID == ServiceManagementId)
             .Select(bs => bs.Booking).Include(x => x.Stylist).ThenInclude(x => x.Person).Include(x => x.Customer).ThenInclude(x => x.Person)
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
        )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.BookingServices
            .Where(bs => bs.ServiceManagementID == ServiceManagementId)
            .Select(bs => bs.Booking).Include(x => x.Stylist).ThenInclude(x => x.Person).Include(x => x.Customer).ThenInclude(x => x.Person)
                   .Where(x => (!x.IsCancelled) &&
       ((!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.IsCancelled.ToString()) && x.IsCancelled.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.Status.ToString()) && x.Status.ToString().Contains(searchText))
      || (!string.IsNullOrEmpty(x.BookingDate.ToString()) && x.BookingDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
      || (!string.IsNullOrEmpty(x.BookingTime.ToString()) && x.BookingTime.ToString("HH:mm").Contains(searchText))
      || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
      || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
       )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public Booking GetBookingById(long BookingId)
        {
            return _context.Bookings.Find(BookingId);
        }

        public void RemoveBooking(Booking Booking)
        {
            _context.Bookings.Remove(Booking);
            _context.SaveChanges();
            _context.Entry(Booking).State = EntityState.Detached;
        }

        public void RemoveBooking(long BookingId)
        {
            var Booking = GetBookingById(BookingId);
            RemoveBooking(Booking);
        }
    }
}
