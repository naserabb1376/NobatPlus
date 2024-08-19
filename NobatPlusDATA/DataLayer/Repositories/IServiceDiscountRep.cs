using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IServiceDiscountRep
    {
        public Task<ListResultObject<ServiceDiscount>> GetAllServiceDiscountsAsync(long DiscountId, long ServiceManagementId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<ServiceDiscount>> GetServiceDiscountByIdAsync(long ServiceDiscountId);
        public Task<BitResultObject> AddServiceDiscountAsync(ServiceDiscount ServiceDiscount);
        public Task<BitResultObject> EditServiceDiscountAsync(ServiceDiscount ServiceDiscount);
        public Task<BitResultObject> RemoveServiceDiscountAsync(ServiceDiscount ServiceDiscount);
        public Task<BitResultObject> RemoveServiceDiscountAsync(long ServiceDiscountId);
        public Task<BitResultObject> ExistServiceDiscountAsync(long ServiceDiscountId);
    }
}
