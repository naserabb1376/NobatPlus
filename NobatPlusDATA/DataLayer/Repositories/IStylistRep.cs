using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistRep
    {
        public Task<List<Stylist>> GetAllStylistsAsync(long parentId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<Stylist>> GetStylistsOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<Stylist>> GetStylistsOfServiceAsync(long serviceManagementId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<Stylist>> GetStylistsOfJobTypeAsync(long JobTypeId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Stylist> GetStylistByIdAsync(long StylistId);
        public Task AddStylistAsync(Stylist Stylist);
        public Task EditStylistAsync(Stylist Stylist);
        public Task RemoveStylistAsync(Stylist Stylist);
        public Task RemoveStylistAsync(long StylistId);
        public Task<bool> ExistStylistAsync(long StylistId);
    }
}
