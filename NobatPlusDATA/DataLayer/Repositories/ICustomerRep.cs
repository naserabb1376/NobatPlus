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
        public List<Customer> GetAllCustomers(int pageIndex = 1,int pageSize = 20, string searchText ="");
        public List<Customer> GetCustomersOfDiscount(long DiscountId,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Customer GetCustomerById(long customerId);
        public void AddCustomer(Customer customer);
        public void EditCustomer(Customer customer);
        public void RemoveCustomer(Customer customer);
        public void RemoveCustomer(long customerId);
        public bool ExistCustomer(long customerId);
    }
}
