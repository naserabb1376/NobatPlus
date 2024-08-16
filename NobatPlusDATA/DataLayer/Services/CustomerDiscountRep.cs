using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class CustomerDiscountRep : ICustomerDiscountRep
    {

        private NobatPlusContext _context;
        public CustomerDiscountRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddCustomerDiscount(CustomerDiscount CustomerDiscount)
        {
            _context.CustomerDiscounts.Add(CustomerDiscount);
            _context.SaveChanges();
            _context.Entry(CustomerDiscount).State = EntityState.Detached;
        }

        public void EditCustomerDiscount(CustomerDiscount CustomerDiscount)
        {
            _context.CustomerDiscounts.Update(CustomerDiscount);
            _context.SaveChanges();
            _context.Entry(CustomerDiscount).State = EntityState.Detached;
        }

        public bool ExistCustomerDiscount(long CustomerDiscountId)
        {
            return _context.CustomerDiscounts.Any(x => x.ID == CustomerDiscountId);
        }

        public List<CustomerDiscount> GetAllCustomerDiscounts(long DiscountId, long CustomerId, long StylistId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
             return _context.CustomerDiscounts.Include(x => x.Discount).Include(x => x.Customer).ThenInclude(x => x.Person).Include(x => x.Stylist).ThenInclude(x => x.Person).Where(x =>
                 (x.DiscountId == DiscountId && x.CustomerId == CustomerId && x.StylistId == StylistId) &&
             ((!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Customer.Person.FirstName.ToString()) && x.Customer.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Customer.Person.LastName.ToString()) && x.Customer.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();


        }

        public CustomerDiscount GetCustomerDiscountById(long CustomerDiscountId)
        {
            return _context.CustomerDiscounts.Find(CustomerDiscountId);
        }

        public void RemoveCustomerDiscount(CustomerDiscount CustomerDiscount)
        {
            _context.CustomerDiscounts.Remove(CustomerDiscount);
            _context.SaveChanges();
            _context.Entry(CustomerDiscount).State = EntityState.Detached;
        }

        public void RemoveCustomerDiscount(long CustomerDiscountId)
        {
            var CustomerDiscount = GetCustomerDiscountById(CustomerDiscountId);
            RemoveCustomerDiscount(CustomerDiscount);
        }
    }
}
