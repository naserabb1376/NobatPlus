using Domain;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistRep
    {
        public Task<ListResultObject<StylistDTO>> GetAllStylistsAsync(long parentId = 0,long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="", FindLocationRequestBody findLocation = null);
        public Task<ListResultObject<StylistDTO>> GetStylistsOfDiscountAsync(long DiscountId,long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="", FindLocationRequestBody findLocation = null);
        public Task<ListResultObject<StylistDTO>> GetStylistsOfServiceAsync(long serviceManagementId,long cityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="", FindLocationRequestBody findLocation = null);
        public Task<ListResultObject<StylistDTO>> GetStylistsOfJobTypeAsync(long JobTypeId, long CityId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="", FindLocationRequestBody findLocation = null);
        public Task<RowResultObject<StylistDTO>> GetStylistByIdAsync(long StylistId);
        public Task<BitResultObject> AddStylistAsync(Stylist Stylist);
        public Task<BitResultObject> EditStylistAsync(Stylist Stylist);
        public Task<BitResultObject> RemoveStylistAsync(Stylist Stylist);
        public Task<BitResultObject> RemoveStylistAsync(long StylistId);
        public Task<BitResultObject> ExistStylistAsync(string fieldValue, string fieldName);
    }
}
