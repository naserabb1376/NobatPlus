using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IBookingServiceRep
    {
        public Task<ListResultObject<BookingService>> GetAllBookingServicesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<BookingService>> GetBookingServiceByIdAsync(long BookingId, long ServiceManagementId);
        public Task<BitResultObject> AddBookingServiceAsync(BookingService BookingService);
        public Task<BitResultObject> EditBookingServiceAsync(BookingService BookingService);
        public Task<BitResultObject> RemoveBookingServiceAsync(BookingService BookingService);
        public Task<BitResultObject> RemoveBookingServiceAsync(long BookingId, long ServiceManagementId);
        public Task<BitResultObject> ExistBookingServiceAsync(long BookingId, long ServiceManagementId);
    }
}
