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
    public interface ICustomerRep
    {
        public Task<ListResultObject<Customer>> GetAllCustomersAsync(long cityId = 0,int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<ListResultObject<Customer>> GetCustomersOfDiscountAsync(long DiscountId,long ciTyId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<RowResultObject<Customer>> GetCustomerByIdAsync(long customerId);
        public Task<BitResultObject> AddCustomerAsync(Customer customer);
        public Task<BitResultObject> EditCustomerAsync(Customer customer);
        public Task<BitResultObject> RemoveCustomerAsync(Customer customer);
        public Task<BitResultObject> RemoveCustomerAsync(long customerId);
        public Task<BitResultObject> ExistCustomerAsync(long customerId);
    }
}
