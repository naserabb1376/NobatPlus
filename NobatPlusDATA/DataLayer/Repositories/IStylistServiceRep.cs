using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistServiceRep
    {
        public Task<List<StylistService>> GetAllStylistServicesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<StylistService> GetStylistServiceByIdAsync(long StylistId, long ServiceManagementId);
        public Task AddStylistServiceAsync(StylistService StylistService);
        public Task EditStylistServiceAsync(StylistService StylistService);
        public Task RemoveStylistServiceAsync(StylistService StylistService);
        public Task RemoveStylistServiceAsync(long StylistId, long ServiceManagementId);
        public Task<bool> ExistStylistServiceAsync(long StylistId, long ServiceManagementId);
    }
}
