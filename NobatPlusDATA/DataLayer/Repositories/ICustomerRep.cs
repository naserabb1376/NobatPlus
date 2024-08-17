using Domain;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ICustomerRep
    {
        public Task<List<Customer>> GetAllCustomersAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<List<Customer>> GetCustomersOfDiscountAsync(long DiscountId, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Customer> GetCustomerByIdAsync(long customerId);
        public Task AddCustomerAsync(Customer customer);
        public Task EditCustomerAsync(Customer customer);
        public Task RemoveCustomerAsync(Customer customer);
        public Task RemoveCustomerAsync(long customerId);
        public Task<bool> ExistCustomerAsync(long customerId);
    }
}
