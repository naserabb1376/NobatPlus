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
        public Task<ListResultObject<StylistDTO>> GetAllStylistsAsync(long parentId = 0, List<long> serviceIds = null, long jobTypeId = 0,long discountId = 0, decimal fromPrice = 0,decimal toPrice = 0, long cityId = 0, int gender = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="", FindLocationRequestBody findLocation = null);
        public Task<RowResultObject<StylistDTO>> GetStylistByIdAsync(long StylistId);
        public Task<BitResultObject> AddStylistAsync(Stylist Stylist);
        public Task<BitResultObject> EditStylistAsync(Stylist Stylist);
        public Task<BitResultObject> RemoveStylistAsync(Stylist Stylist);
        public Task<BitResultObject> RemoveStylistAsync(long StylistId);
        public Task<BitResultObject> ExistStylistAsync(string fieldValue, string fieldName);
    }
}
