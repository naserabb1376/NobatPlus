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

        public async Task<BitResultObject> AddDiscountAsync(Discount Discount)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Discounts.Add(Discount);
                await _context.SaveChangesAsync();
                _context.Entry(Discount).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditDiscountAsync(Discount Discount)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Discounts.Update(Discount);
                await _context.SaveChangesAsync();
                _context.Entry(Discount).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistDiscountAsync(long DiscountId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Discounts
                .AsNoTracking()
                .AnyAsync(x => x.ID == DiscountId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<Discount>> GetAllDiscountsAsync(DiscountType discountType = DiscountType.All, long AdminId = 0, long StylistId = 0, long CustomerId = 0, long ServiceManagementId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Discount> results = new ListResultObject<Discount>();
            try
            {
                IQueryable<Discount> query;

                switch (discountType)
                {
                    case DiscountType.All:
                    default:
                        query = _context.Discounts
                            .AsNoTracking()
                            .Where(x =>
                                (!string.IsNullOrEmpty(x.DiscountCode) && x.DiscountCode.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.DiscountAmount.ToString()) && x.DiscountAmount.ToString().Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.StartDate.ToString()) && x.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.EndDate.ToString()) && x.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                            );
                        break;
                    case DiscountType.Admin:
                        query = _context.DiscountAssignments
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
                            );
                        break;
                    case DiscountType.Customer:
                        query = _context.CustomerDiscounts
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
                            );
                        break;
                    case DiscountType.Stylist:
                        query = _context.DiscountAssignments
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
                            );
                        break;
                    case DiscountType.StylistCustomer:
                        query = _context.CustomerDiscounts
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
                            );
                        break;
                    case DiscountType.Service:
                        query = _context.ServiceDiscounts
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
                            );
                        break;
                    case DiscountType.AdminService:
                        query = _context.ServiceDiscounts
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
                            );
                        break;
                    case DiscountType.StylistService:
                        query = _context.ServiceDiscounts
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
                            );
                        break;
                }

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
            
        }

        public async Task<RowResultObject<Discount>> GetDiscountByIdAsync(long DiscountId)
        {
            RowResultObject<Discount> result = new RowResultObject<Discount>();
            try
            {
                result.Result = await _context.Discounts
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == DiscountId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveDiscountAsync(Discount Discount)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Discounts.Remove(Discount);
                await _context.SaveChangesAsync();
                _context.Entry(Discount).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveDiscountAsync(long DiscountId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Discount = await GetDiscountByIdAsync(DiscountId);
                result = await RemoveDiscountAsync(Discount.Result);
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
