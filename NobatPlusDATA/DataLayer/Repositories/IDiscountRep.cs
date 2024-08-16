using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IDiscountRep
    {
        public List<Discount> GetAllDiscounts(DiscountType discountType = DiscountType.All, long AdminId = 0, long StylistId = 0, long CustomerId = 0, long ServiceManagementId = 0,int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Discount GetDiscountById(long DiscountId);
        public void AddDiscount(Discount Discount);
        public void EditDiscount(Discount Discount);
        public void RemoveDiscount(Discount Discount);
        public void RemoveDiscount(long DiscountId);
        public bool ExistDiscount(long DiscountId);
    }
}
