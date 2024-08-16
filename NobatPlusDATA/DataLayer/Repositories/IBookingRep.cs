using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IBookingRep
    {
        public List<Booking> GetAllBookings(int cancelState = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<Booking> GetBookingsOfService(int ServiceManagementId,int cancelState = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Booking GetBookingById(long BookingId);
        public void AddBooking(Booking Booking);
        public void EditBooking(Booking Booking);
        public void RemoveBooking(Booking Booking);
        public void RemoveBooking(long BookingId);
        public bool ExistBooking(long BookingId);
    }
}
