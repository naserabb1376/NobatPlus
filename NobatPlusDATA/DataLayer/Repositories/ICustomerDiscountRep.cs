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
        public List<CustomerDiscount> GetAllCustomerDiscounts(long DiscountId, long CustomerId,long StylistId,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public CustomerDiscount GetCustomerDiscountById(long CustomerDiscountId);
        public void AddCustomerDiscount(CustomerDiscount CustomerDiscount);
        public void EditCustomerDiscount(CustomerDiscount CustomerDiscount);
        public void RemoveCustomerDiscount(CustomerDiscount CustomerDiscount);
        public void RemoveCustomerDiscount(long CustomerDiscountId);
        public bool ExistCustomerDiscount(long CustomerDiscountId);
    }
}
