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

        public async Task AddDiscountAsync(Discount Discount)
        {
            _context.Discounts.Add(Discount);
            await _context.SaveChangesAsync();
            _context.Entry(Discount).State = EntityState.Detached;
        }

        public async Task EditDiscountAsync(Discount Discount)
        {
            _context.Discounts.Update(Discount);
            await _context.SaveChangesAsync();
            _context.Entry(Discount).State = EntityState.Detached;
        }

        public async Task<bool> ExistDiscountAsync(long DiscountId)
        {
            return await _context.Discounts
                .AsNoTracking()
                .AnyAsync(x => x.ID == DiscountId);
        }

        public async Task<List<Discount>> GetAllDiscountsAsync(DiscountType discountType = DiscountType.All, long AdminId = 0, long StylistId = 0, long CustomerId = 0, long ServiceManagementId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            switch (discountType)
            {
                case DiscountType.All:
                default:
                    return await _context.Discounts
                        .AsNoTracking()
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                        .OrderByDescending(x => x.CreateDate)
                        .ToPaging(pageIndex, pageSize)
                        .ToListAsync();
                case DiscountType.Admin:
                    return await _context.DiscountAssignments
                        .AsNoTracking()
                        .Where(bs => bs.AdminId == AdminId)
                        .Select(bs => bs.Discount)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                        .OrderByDescending(x => x.CreateDate)
                        .ToPaging(pageIndex, pageSize)
                        .ToListAsync();
                case DiscountType.Customer:
                    return await _context.CustomerDiscounts
                        .AsNoTracking()
                        .Where(bs => bs.CustomerId == CustomerId)
                        .Select(bs => bs.Discount)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                        .OrderByDescending(x => x.CreateDate)
                        .ToPaging(pageIndex, pageSize)
                        .ToListAsync();
                case DiscountType.Stylist:
                    return await _context.DiscountAssignments
                        .AsNoTracking()
                        .Where(bs => bs.StylistId == StylistId)
                        .Select(bs => bs.Discount)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                        .OrderByDescending(x => x.CreateDate)
                        .ToPaging(pageIndex, pageSize)
                        .ToListAsync();
                case DiscountType.StylistCustomer:
                    return await _context.CustomerDiscounts
                        .AsNoTracking()
                        .Where(bs => bs.StylistId == StylistId && bs.CustomerId == CustomerId)
                        .Select(bs => bs.Discount)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                        .OrderByDescending(x => x.CreateDate)
                        .ToPaging(pageIndex, pageSize)
                        .ToListAsync();
                case DiscountType.Service:
                    return await _context.ServiceDiscounts
                        .AsNoTracking()
                        .Where(bs => bs.ServiceManagementId == ServiceManagementId)
                        .Select(bs => bs.Discount)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                        .OrderByDescending(x => x.CreateDate)
                        .ToPaging(pageIndex, pageSize)
                        .ToListAsync();
                case DiscountType.AdminService:
                    return await _context.ServiceDiscounts
                        .AsNoTracking()
                        .Where(bs => bs.AdminId == AdminId && bs.ServiceManagementId == ServiceManagementId)
                        .Select(bs => bs.Discount)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                        .OrderByDescending(x => x.CreateDate)
                        .ToPaging(pageIndex, pageSize)
                        .ToListAsync();
                case DiscountType.StylistService:
                    return await _context.ServiceDiscounts
                        .AsNoTracking()
                        .Where(bs => bs.StylistId == StylistId && bs.ServiceManagementId == ServiceManagementId)
                        .Select(bs => bs.Discount)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                        )
                        .OrderByDescending(x => x.CreateDate)
                        .ToPaging(pageIndex, pageSize)
                        .ToListAsync();
            }
        }

        public async Task<Discount> GetDiscountByIdAsync(long DiscountId)
        {
            return await _context.Discounts
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == DiscountId);
        }

        public async Task RemoveDiscountAsync(Discount Discount)
        {
            _context.Discounts.Remove(Discount);
            await _context.SaveChangesAsync();
            _context.Entry(Discount).State = EntityState.Detached;
        }

        public async Task RemoveDiscountAsync(long DiscountId)
        {
            var Discount = await GetDiscountByIdAsync(DiscountId);
            await RemoveDiscountAsync(Discount);
        }
    }
}
