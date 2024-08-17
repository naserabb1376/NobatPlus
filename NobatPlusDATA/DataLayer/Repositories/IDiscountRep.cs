using NobatPlusDATA.Domain;
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
        public Task<List<Discount>> GetAllDiscountsAsync(DiscountType discountType = DiscountType.All, long AdminId = 0, long StylistId = 0, long CustomerId = 0, long ServiceManagementId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Discount> GetDiscountByIdAsync(long DiscountId);
        public Task AddDiscountAsync(Discount Discount);
        public Task EditDiscountAsync(Discount Discount);
        public Task RemoveDiscountAsync(Discount Discount);
        public Task RemoveDiscountAsync(long DiscountId);
        public Task<bool> ExistDiscountAsync(long DiscountId);
    }
}
