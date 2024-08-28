using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ICustomerDiscountRep
    {
        public Task<ListResultObject<CustomerDiscount>> GetAllCustomerDiscountsAsync(long DiscountId, long CustomerId, long StylistId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<CustomerDiscount>> GetCustomerDiscountByIdAsync(long CustomerDiscountId);
        public Task<BitResultObject> AddCustomerDiscountsAsync(List<CustomerDiscount> customerDiscounts);
        public Task<BitResultObject> EditCustomerDiscountsAsync(List<CustomerDiscount> customerDiscounts);
        public Task<BitResultObject> RemoveCustomerDiscountsAsync(List<CustomerDiscount> customerDiscounts);
        public Task<BitResultObject> RemoveCustomerDiscountsAsync(List<long> customerDiscountIds);
        public Task<BitResultObject> ExistCustomerDiscountAsync(long CustomerDiscountId);
    }
}
