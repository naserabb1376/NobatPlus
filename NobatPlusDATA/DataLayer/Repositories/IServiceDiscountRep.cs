using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IServiceDiscountRep
    {
        public Task<List<ServiceDiscount>> GetAllServiceDiscountsAsync(long DiscountId, long ServiceManagementId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<ServiceDiscount> GetServiceDiscountByIdAsync(long ServiceDiscountId);
        public Task AddServiceDiscountAsync(ServiceDiscount ServiceDiscount);
        public Task EditServiceDiscountAsync(ServiceDiscount ServiceDiscount);
        public Task RemoveServiceDiscountAsync(ServiceDiscount ServiceDiscount);
        public Task RemoveServiceDiscountAsync(long ServiceDiscountId);
        public Task<bool> ExistServiceDiscountAsync(long ServiceDiscountId);
    }
}
