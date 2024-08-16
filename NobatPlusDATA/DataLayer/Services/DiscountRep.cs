using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class DiscountRep : IDiscountRep
    {

        private NobatPlusContext _context;
        public DiscountRep()
        {
            _context = DbTools.GetDbContext();
        }

        public void AddDiscount(Discount Discount)
        {
            _context.Discounts.Add(Discount);
            _context.SaveChanges();
            _context.Entry(Discount).State = EntityState.Detached;
        }

        public void EditDiscount(Discount Discount)
        {
            _context.Discounts.Update(Discount);
            _context.SaveChanges();
            _context.Entry(Discount).State = EntityState.Detached;
        }

        public bool ExistDiscount(long DiscountId)
        {
            return _context.Discounts.Any(x => x.ID == DiscountId);
        }

        public List<Discount> GetAllDiscounts(DiscountType discountType = DiscountType.All,long AdminId = 0,long StylistId = 0,long CustomerId = 0, long ServiceManagementId = 0 ,int pageIndex= 1, int pageSize = 20, string searchText= "")
        {
            switch (discountType)
            {
                case DiscountType.All:
                default:
                    return _context.Discounts.Where(x =>
              (!string.IsNullOrEmpty(x.DiscountCode.ToString()) && x.DiscountCode.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
                case DiscountType.Admin:
                    return _context.DiscountAssignments
           .Where(bs => bs.AdminId == AdminId)
           .Select(bs => bs.Discount).Where(x =>
              (!string.IsNullOrEmpty(x.DiscountCode.ToString()) && x.DiscountCode.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
                case DiscountType.Customer:
                    return _context.CustomerDiscounts
            .Where(bs => bs.CustomerId == CustomerId)
            .Select(bs => bs.Discount).Where(x =>
               (!string.IsNullOrEmpty(x.DiscountCode.ToString()) && x.DiscountCode.ToString().Contains(searchText))
              || (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText))
              || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
              || (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              || (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
               ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
                case DiscountType.Stylist:
                    return _context.DiscountAssignments
           .Where(bs => bs.StylistId == StylistId)
           .Select(bs => bs.Discount).Where(x =>
              (!string.IsNullOrEmpty(x.DiscountCode.ToString()) && x.DiscountCode.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
                case DiscountType.StylistCustomer:
                    return _context.CustomerDiscounts
            .Where(bs => bs.StylistId == StylistId && bs.CustomerId == CustomerId)
            .Select(bs => bs.Discount).Where(x =>
               (!string.IsNullOrEmpty(x.DiscountCode.ToString()) && x.DiscountCode.ToString().Contains(searchText))
              || (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText))
              || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
              || (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              || (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
               ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
                case DiscountType.Service:
                    return _context.ServiceDiscounts
           .Where(bs => bs.ServiceManagementId == ServiceManagementId)
           .Select(bs => bs.Discount).Where(x =>
              (!string.IsNullOrEmpty(x.DiscountCode.ToString()) && x.DiscountCode.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
             || (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
             || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
              ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
                case DiscountType.AdminService:
                    return _context.ServiceDiscounts
                               .Where(bs => bs.AdminId == AdminId && bs.ServiceManagementId == ServiceManagementId)
                               .Select(bs => bs.Discount).Where(x =>
                                  (!string.IsNullOrEmpty(x.DiscountCode.ToString()) && x.DiscountCode.ToString().Contains(searchText))
                                 || (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText))
                                 || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                                 || (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                                 || (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                                 || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                                 || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                                  ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
                case DiscountType.StylistService:
                    return _context.ServiceDiscounts
                              .Where(bs => bs.StylistId == StylistId && bs.ServiceManagementId == ServiceManagementId)
                              .Select(bs => bs.Discount).Where(x =>
                                 (!string.IsNullOrEmpty(x.DiscountCode.ToString()) && x.DiscountCode.ToString().Contains(searchText))
                                || (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText))
                                || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                                || (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                                || (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                                || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                                || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                                 ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex, pageSize).ToList();
            }
        }

        public Discount GetDiscountById(long DiscountId)
        {
            return _context.Discounts.Find(DiscountId);
        }

        public void RemoveDiscount(Discount Discount)
        {
            _context.Discounts.Remove(Discount);
            _context.SaveChanges();
            _context.Entry(Discount).State = EntityState.Detached;
        }

        public void RemoveDiscount(long DiscountId)
        {
            var Discount = GetDiscountById(DiscountId);
            RemoveDiscount(Discount);
        }
    }
}
