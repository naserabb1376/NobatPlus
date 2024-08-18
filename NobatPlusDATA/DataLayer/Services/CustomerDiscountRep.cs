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
    public class CustomerDiscountRep : ICustomerDiscountRep
    {

        private NobatPlusContext _context;
        public CustomerDiscountRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddCustomerDiscountAsync(CustomerDiscount CustomerDiscount)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.CustomerDiscounts.Add(CustomerDiscount);
                await _context.SaveChangesAsync();
                _context.Entry(CustomerDiscount).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditCustomerDiscountAsync(CustomerDiscount CustomerDiscount)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.CustomerDiscounts.Update(CustomerDiscount);
                await _context.SaveChangesAsync();
                _context.Entry(CustomerDiscount).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistCustomerDiscountAsync(long CustomerDiscountId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.CustomerDiscounts
                .AsNoTracking()
                .AnyAsync(x => x.ID == CustomerDiscountId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<CustomerDiscount>> GetAllCustomerDiscountsAsync(long DiscountId, long CustomerId, long StylistId, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<CustomerDiscount> results = new ListResultObject<CustomerDiscount>();
            try
            {
                var query = _context.CustomerDiscounts
                .AsNoTracking()
                .Include(x => x.Discount)
                .Include(x => x.Customer).ThenInclude(x => x.Person)
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Where(x =>
                    (x.DiscountId == DiscountId && x.CustomerId == CustomerId && x.StylistId == StylistId) &&
                    (
                        (!string.IsNullOrEmpty(x.Discount.DiscountCode.ToString()) && x.Discount.DiscountCode.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountAmount.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.Description.ToString()) && x.Discount.Description.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Customer.Person.FirstName.ToString()) && x.Customer.Person.FirstName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Customer.Person.LastName.ToString()) && x.Customer.Person.LastName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                    )
                );
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

        public async Task<RowResultObject<CustomerDiscount>> GetCustomerDiscountByIdAsync(long CustomerDiscountId)
        {
            RowResultObject<CustomerDiscount> result = new RowResultObject<CustomerDiscount>();
            try
            {
                result.Result = await _context.CustomerDiscounts
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == CustomerDiscountId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveCustomerDiscountAsync(CustomerDiscount CustomerDiscount)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.CustomerDiscounts.Remove(CustomerDiscount);
                await _context.SaveChangesAsync();
                _context.Entry(CustomerDiscount).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveCustomerDiscountAsync(long CustomerDiscountId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var CustomerDiscount = await GetCustomerDiscountByIdAsync(CustomerDiscountId);
                result = await RemoveCustomerDiscountAsync(CustomerDiscount.Result);
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
