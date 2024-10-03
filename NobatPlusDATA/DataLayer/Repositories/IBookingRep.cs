using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IBookingRep
    {
        public Task<ListResultObject<Booking>> GetAllBookingsAsync(int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<ListResultObject<Booking>> GetBookingsOfServiceAsync(long ServiceManagementId, int cancelState = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<Booking>> GetBookingByIdAsync(long BookingId);
        public Task<BitResultObject> AddBookingAsync(Booking Booking);
        public Task<BitResultObject> EditBookingAsync(Booking Booking);
        public Task<BitResultObject> RemoveBookingAsync(Booking Booking);
        public Task<BitResultObject> RemoveBookingAsync(long BookingId);
        public Task<BitResultObject> ExistBookingAsync(long BookingId);
    }
}
