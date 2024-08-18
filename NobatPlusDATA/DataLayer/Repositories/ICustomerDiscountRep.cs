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
        public Task<BitResultObject> AddCustomerDiscountAsync(CustomerDiscount CustomerDiscount);
        public Task<BitResultObject> EditCustomerDiscountAsync(CustomerDiscount CustomerDiscount);
        public Task<BitResultObject> RemoveCustomerDiscountAsync(CustomerDiscount CustomerDiscount);
        public Task<BitResultObject> RemoveCustomerDiscountAsync(long CustomerDiscountId);
        public Task<BitResultObject> ExistCustomerDiscountAsync(long CustomerDiscountId);
    }
}
