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
        public Task<List<Booking>> GetAllBookingsAsync(int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<Booking>> GetBookingsOfServiceAsync(int ServiceManagementId, int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Booking> GetBookingByIdAsync(long BookingId);
        public Task AddBookingAsync(Booking Booking);
        public Task EditBookingAsync(Booking Booking);
        public Task RemoveBookingAsync(Booking Booking);
        public Task RemoveBookingAsync(long BookingId);
        public Task<bool> ExistBookingAsync(long BookingId);
    }
}
