using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ICustomerDiscountRep
    {
        public Task<List<CustomerDiscount>> GetAllCustomerDiscountsAsync(long DiscountId, long CustomerId, long StylistId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<CustomerDiscount> GetCustomerDiscountByIdAsync(long CustomerDiscountId);
        public Task AddCustomerDiscountAsync(CustomerDiscount CustomerDiscount);
        public Task EditCustomerDiscountAsync(CustomerDiscount CustomerDiscount);
        public Task RemoveCustomerDiscountAsync(CustomerDiscount CustomerDiscount);
        public Task RemoveCustomerDiscountAsync(long CustomerDiscountId);
        public Task<bool> ExistCustomerDiscountAsync(long CustomerDiscountId);
    }
}
