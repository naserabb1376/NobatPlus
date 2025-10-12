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
        public Task<ListResultObject<CustomerDTO>> GetAllCustomersAsync(long stylistId=0,long cityId = 0,long discountId =0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<CustomerDTO>> GetCustomerByIdAsync(long customerId);
        public Task<BitResultObject> AddCustomerAsync(Customer customer);
        public Task<BitResultObject> EditCustomerAsync(Customer customer);
        public Task<BitResultObject> RemoveCustomerAsync(Customer customer);
        public Task<BitResultObject> RemoveCustomerAsync(long customerId);
        public Task<BitResultObject> ExistCustomerAsync(long customerId);
    }
}
