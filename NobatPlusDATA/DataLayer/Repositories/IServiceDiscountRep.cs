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
        public List<ServiceDiscount> GetAllServiceDiscounts(long DiscountId, long ServiceManagementId, long AdminId = 0 , long StylistId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public ServiceDiscount GetServiceDiscountById(long ServiceDiscountId);
        public void AddServiceDiscount(ServiceDiscount ServiceDiscount);
        public void EditServiceDiscount(ServiceDiscount ServiceDiscount);
        public void RemoveServiceDiscount(ServiceDiscount ServiceDiscount);
        public void RemoveServiceDiscount(long ServiceDiscountId);
        public bool ExistServiceDiscount(long ServiceDiscountId);
    }
}
