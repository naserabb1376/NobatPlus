using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IBookingServiceRep
    {
        public Task<List<BookingService>> GetAllBookingServicesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<BookingService> GetBookingServiceByIdAsync(long BookingId, long ServiceManagementId);
        public Task AddBookingServiceAsync(BookingService BookingService);
        public Task EditBookingServiceAsync(BookingService BookingService);
        public Task RemoveBookingServiceAsync(BookingService BookingService);
        public Task RemoveBookingServiceAsync(long BookingId, long ServiceManagementId);
        public Task<bool> ExistBookingServiceAsync(long BookingId, long ServiceManagementId);
    }
}
