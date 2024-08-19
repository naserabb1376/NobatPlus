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
        public Task<BitResultObject> AddStylistServiceAsync(StylistService StylistService);
        public Task<BitResultObject> EditStylistServiceAsync(StylistService StylistService);
        public Task<BitResultObject> RemoveStylistServiceAsync(StylistService StylistService);
        public Task<BitResultObject> RemoveStylistServiceAsync(long StylistId, long ServiceManagementId);
        public Task<BitResultObject> ExistStylistServiceAsync(long StylistId, long ServiceManagementId);
    }
}
