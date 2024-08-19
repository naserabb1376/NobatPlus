using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IServiceManagementRep
    {
        public Task<ListResultObject<ServiceManagement>> GetAllServiceManagementsAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<ListResultObject<ServiceManagement>> GetServicesOfBookingAsync(long bookingId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<ListResultObject<ServiceManagement>> GetServicesOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<ListResultObject<ServiceManagement>> GetServicesOfStylistAsync(long stylistId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<ServiceManagement>> GetServiceManagementByIdAsync(long ServiceManagementId);
        public Task<BitResultObject> AddServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task<BitResultObject> EditServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task<BitResultObject> RemoveServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task<BitResultObject> RemoveServiceManagementAsync(long ServiceManagementId);
        public Task<BitResultObject> ExistServiceManagementAsync(long ServiceManagementId);
    }
}
