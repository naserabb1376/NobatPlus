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

        public async Task AddServiceDiscountAsync(ServiceDiscount ServiceDiscount)
        {
            _context.ServiceDiscounts.Add(ServiceDiscount);
            await _context.SaveChangesAsync();
            _context.Entry(ServiceDiscount).State = EntityState.Detached;
        }

        public async Task EditServiceDiscountAsync(ServiceDiscount ServiceDiscount)
        {
            _context.ServiceDiscounts.Update(ServiceDiscount);
            await _context.SaveChangesAsync();
            _context.Entry(ServiceDiscount).State = EntityState.Detached;
        }

        public async Task<bool> ExistServiceDiscountAsync(long ServiceDiscountId)
        {
            return await _context.ServiceDiscounts
                .AsNoTracking()
                .AnyAsync(x => x.ID == ServiceDiscountId);
        }

        public async Task<List<ServiceDiscount>> GetAllServiceDiscountsAsync(long DiscountId, long ServiceManagementId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            IQueryable<ServiceDiscount> query = _context.ServiceDiscounts
                .AsNoTracking()
                .Include(x => x.Discount)
                .Include(x => x.Admin).ThenInclude(x => x.Person)
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement);

            if (AdminId > 0)
            {
                query = query.Where(x => x.DiscountId == DiscountId && x.ServiceManagementId == ServiceManagementId && x.AdminId == AdminId);
            }
            else if (StylistId > 0)
            {
                query = query.Where(x => x.DiscountId == DiscountId && x.ServiceManagementId == ServiceManagementId && x.StylistId == StylistId);
            }
            else
            {
                query = query.Where(x => x.DiscountId == DiscountId && x.ServiceManagementId == ServiceManagementId);
            }

            query = query.Where(x =>
                (!string.IsNullOrEmpty(x.Discount.DiscountCode) && x.Discount.DiscountCode.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountAmount.ToString().Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Discount.Description) && x.Discount.Description.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Admin.Person.FirstName) && x.Admin.Person.FirstName.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Admin.Person.LastName) && x.Admin.Person.LastName.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName) && x.ServiceManagement.ServiceName.Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.ServiceManagement.Price.ToString()) && x.ServiceManagement.Price.ToString().Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            );

            return await query
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<ServiceDiscount> GetServiceDiscountByIdAsync(long ServiceDiscountId)
        {
            return await _context.ServiceDiscounts
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == ServiceDiscountId);
        }

        public async Task RemoveServiceDiscountAsync(ServiceDiscount ServiceDiscount)
        {
            _context.ServiceDiscounts.Remove(ServiceDiscount);
            await _context.SaveChangesAsync();
            _context.Entry(ServiceDiscount).State = EntityState.Detached;
        }

        public async Task RemoveServiceDiscountAsync(long ServiceDiscountId)
        {
            var serviceDiscount = await GetServiceDiscountByIdAsync(ServiceDiscountId);
            await RemoveServiceDiscountAsync(serviceDiscount);
        }
    }
}
