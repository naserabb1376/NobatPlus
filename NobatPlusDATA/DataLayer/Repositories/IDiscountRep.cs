using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IDiscountRep
    {
        public Task<ListResultObject<Discount>> GetAllDiscountsAsync(DiscountType discountType = DiscountType.All, long AdminId = 0, long StylistId = 0, long CustomerId = 0, long ServiceManagementId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<Discount>> GetDiscountByIdAsync(long DiscountId);
        public Task<BitResultObject> AddDiscountAsync(Discount Discount);
        public Task<BitResultObject> EditDiscountAsync(Discount Discount);
        public Task<BitResultObject> RemoveDiscountAsync(Discount Discount);
        public Task<BitResultObject> RemoveDiscountAsync(long DiscountId);
        public Task<BitResultObject> ExistDiscountAsync(long DiscountId);
    }
}
