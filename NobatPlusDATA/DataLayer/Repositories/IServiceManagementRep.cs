using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IServiceManagementRep
    {
        public Task<List<ServiceManagement>> GetAllServiceManagementsAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<ServiceManagement>> GetServicesOfBookingAsync(long bookingId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<ServiceManagement>> GetServicesOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<ServiceManagement>> GetServicesOfStylistAsync(long stylistId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<ServiceManagement> GetServiceManagementByIdAsync(long ServiceManagementId);
        public Task AddServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task EditServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task RemoveServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task RemoveServiceManagementAsync(long ServiceManagementId);
        public Task<bool> ExistServiceManagementAsync(long ServiceManagementId);
    }
}
