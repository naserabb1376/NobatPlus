using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
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
        public ServiceDiscountRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddServiceDiscountsAsync(List<ServiceDiscount> serviceDiscounts)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.ServiceDiscounts.AddRangeAsync(serviceDiscounts);
                await _context.SaveChangesAsync();
                result.ID = serviceDiscounts.FirstOrDefault().ID;
                foreach (var serviceDiscount in serviceDiscounts)
                {
                    _context.Entry(serviceDiscount).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> EditServiceDiscountsAsync(List<ServiceDiscount> serviceDiscounts)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.ServiceDiscounts.UpdateRange(serviceDiscounts);
                await _context.SaveChangesAsync();
                result.ID = serviceDiscounts.FirstOrDefault().ID;
                foreach (var serviceDiscount in serviceDiscounts)
                {
                    _context.Entry(serviceDiscount).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> RemoveServiceDiscountsAsync(List<ServiceDiscount> serviceDiscounts)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.ServiceDiscounts.RemoveRange(serviceDiscounts);
                await _context.SaveChangesAsync();
                result.ID = serviceDiscounts.FirstOrDefault().ID;
                foreach (var serviceDiscount in serviceDiscounts)
                {
                    _context.Entry(serviceDiscount).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> RemoveServiceDiscountsAsync(List<long> serviceDiscountIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var serviceDiscountsToRemove = new List<ServiceDiscount>();

                foreach (var serviceDiscountId in serviceDiscountIds)
                {
                    var serviceDiscount = await GetServiceDiscountByIdAsync(serviceDiscountId);
                    if (serviceDiscount.Result != null)
                    {
                        serviceDiscountsToRemove.Add(serviceDiscount.Result);
                    }
                }

                if (serviceDiscountsToRemove.Any())
                {
                    result = await RemoveServiceDiscountsAsync(serviceDiscountsToRemove);
                }
                else
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching service discounts found to remove.";
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }



        public async Task<BitResultObject> ExistServiceDiscountAsync(long ServiceDiscountId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.ServiceDiscounts
                .AsNoTracking()
                .AnyAsync(x => x.ID == ServiceDiscountId);
                result.ID = ServiceDiscountId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<ServiceDiscount>> GetAllServiceDiscountsAsync(long DiscountId, long ServiceManagementId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<ServiceDiscount> results = new ListResultObject<ServiceDiscount>();
            try
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
                    (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Admin.Person.FirstName) && x.Admin.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Admin.Person.LastName) && x.Admin.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Admin.Role) && x.Admin.Role.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName) && x.ServiceManagement.ServiceName.Contains(searchText)) ||
                    //(!string.IsNullOrEmpty(x.ServiceManagement.Price.ToString()) && x.ServiceManagement.Price.ToString().Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
         

            
        }

        public async Task<RowResultObject<ServiceDiscount>> GetServiceDiscountByIdAsync(long ServiceDiscountId)
        {
            RowResultObject<ServiceDiscount> result = new RowResultObject<ServiceDiscount>();
            try
            {
                result.Result = await _context.ServiceDiscounts
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == ServiceDiscountId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }
    }
}
