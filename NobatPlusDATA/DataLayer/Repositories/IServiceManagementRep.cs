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
        public Task<ListResultObject<ServiceManagement>> GetAllServiceManagementsAsync(long parentId = 0, long bookingId = 0, long stylistId = 0, long discountId = 0, char serviceGender=' ', int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<ServiceManagement>> GetServiceManagementByIdAsync(long ServiceManagementId);
        public Task<BitResultObject> AddServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task<BitResultObject> EditServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task<BitResultObject> RemoveServiceManagementAsync(ServiceManagement ServiceManagement);
        public Task<BitResultObject> RemoveServiceManagementAsync(long ServiceManagementId);
        public Task<BitResultObject> ExistServiceManagementAsync(long ServiceManagementId);
    }
}
