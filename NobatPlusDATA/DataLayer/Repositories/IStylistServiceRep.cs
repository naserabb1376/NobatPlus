using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistServiceRep
    {
        public Task<ListResultObject<StylistService>> GetAllStylistServicesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<StylistService>> GetStylistServiceByIdAsync(long StylistId, long ServiceManagementId);
        public Task<BitResultObject> AddStylistServicesAsync(List<StylistService> stylistServices);
        public Task<BitResultObject> EditStylistServicesAsync(List<StylistService> stylistServices);
        public Task<BitResultObject> RemoveStylistServicesAsync(List<StylistService> stylistServices);
        public Task<BitResultObject> RemoveStylistServicesAsync(List<(long StylistId, long ServiceManagementId)> stylistServiceIds);
        public Task<BitResultObject> ExistStylistServiceAsync(long StylistId, long ServiceManagementId);
    }
}
