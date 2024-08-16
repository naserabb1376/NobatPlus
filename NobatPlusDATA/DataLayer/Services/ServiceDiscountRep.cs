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
    public class ServiceDiscountRep : IServiceDiscountRep
    {

        private NobatPlusContext _context;
        public ServiceDiscountRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddServiceDiscount(ServiceDiscount ServiceDiscount)
        {
            _context.ServiceDiscounts.Add(ServiceDiscount);
            _context.SaveChanges();
            _context.Entry(ServiceDiscount).State = EntityState.Detached;
        }

        public void EditServiceDiscount(ServiceDiscount ServiceDiscount)
        {
            _context.ServiceDiscounts.Update(ServiceDiscount);
            _context.SaveChanges();
            _context.Entry(ServiceDiscount).State = EntityState.Detached;
        }

        public bool ExistServiceDiscount(long ServiceDiscountId)
        {
            return _context.ServiceDiscounts.Any(x => x.ID == ServiceDiscountId);
        }

        public List<ServiceDiscount> GetAllServiceDiscounts(long DiscountId, long ServiceManagementId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            if (AdminId > 0)
            {
                return _context.ServiceDiscounts.Include(x => x.Discount).Include(x => x.Admin).ThenInclude(x => x.Person).Include(x => x.Stylist).ThenInclude(x => x.Person).Where(x =>
                 (x.DiscountId == DiscountId && x.ServiceManagementId == ServiceManagementId && x.AdminId == AdminId) &&
             ((!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.Admin.Person.FirstName.ToString()) && x.Admin.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Admin.Person.LastName.ToString()) && x.Admin.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName.ToString()) && x.ServiceManagement.ServiceName.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.ServiceManagement.Price.ToString()) && x.ServiceManagement.Price.ToString().Contains(searchText))
            || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }

            if (StylistId > 0)
            {
                return _context.ServiceDiscounts.Include(x => x.Discount).Include(x => x.Admin).ThenInclude(x => x.Person).Include(x => x.Stylist).ThenInclude(x => x.Person).Where(x =>
                (x.DiscountId == DiscountId && x.ServiceManagementId == ServiceManagementId && x.StylistId == StylistId) &&
            ((!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.Admin.Person.FirstName.ToString()) && x.Admin.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Admin.Person.LastName.ToString()) && x.Admin.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName.ToString()) && x.ServiceManagement.ServiceName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.ServiceManagement.Price.ToString()) && x.ServiceManagement.Price.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
            else
            {
                return _context.ServiceDiscounts.Include(x => x.Discount).Include(x => x.Admin).ThenInclude(x => x.Person).Include(x => x.Stylist).ThenInclude(x => x.Person).Where(x =>
                (x.DiscountId == DiscountId && x.ServiceManagementId == ServiceManagementId ) &&
          ((!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
         || (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
         || (!string.IsNullOrEmpty(x.Admin.Person.FirstName.ToString()) && x.Admin.Person.FirstName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Admin.Person.LastName.ToString()) && x.Admin.Person.LastName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName.ToString()) && x.ServiceManagement.ServiceName.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.ServiceManagement.Price.ToString()) && x.ServiceManagement.Price.ToString().Contains(searchText))
         || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
         || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
          )).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public ServiceDiscount GetServiceDiscountById(long ServiceDiscountId)
        {
            return _context.ServiceDiscounts.Find(ServiceDiscountId);
        }

        public void RemoveServiceDiscount(ServiceDiscount ServiceDiscount)
        {
            _context.ServiceDiscounts.Remove(ServiceDiscount);
            _context.SaveChanges();
            _context.Entry(ServiceDiscount).State = EntityState.Detached;
        }

        public void RemoveServiceDiscount(long ServiceDiscountId)
        {
            var ServiceDiscount = GetServiceDiscountById(ServiceDiscountId);
            RemoveServiceDiscount(ServiceDiscount);
        }
    }
}
