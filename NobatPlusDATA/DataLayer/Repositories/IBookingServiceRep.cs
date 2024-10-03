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
        public Task<ListResultObject<BookingService>> GetAllBookingServicesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<BookingService>> GetBookingServiceByIdAsync(long BookingId, long ServiceManagementId);
        public Task<BitResultObject> AddBookingServicesAsync(List<BookingService> bookingServices);
        public Task<BitResultObject> EditBookingServicesAsync(List<BookingService> bookingServices);
        public Task<BitResultObject> RemoveBookingServicesAsync(List<BookingService> bookingServices);
        public Task<BitResultObject> RemoveBookingServicesAsync(List<(long BookingId, long ServiceManagementId)> bookingServiceIds);
        public Task<BitResultObject> ExistBookingServiceAsync(long BookingId, long ServiceManagementId);
    }
}
