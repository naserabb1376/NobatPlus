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

        public async Task<BitResultObject> AddCustomerDiscountsAsync(List<CustomerDiscount> customerDiscounts)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.CustomerDiscounts.AddRangeAsync(customerDiscounts);
                await _context.SaveChangesAsync();
                result.ID = customerDiscounts.FirstOrDefault().ID;
                foreach (var customerDiscount in customerDiscounts)
                {
                    _context.Entry(customerDiscount).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> EditCustomerDiscountsAsync(List<CustomerDiscount> customerDiscounts)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.CustomerDiscounts.UpdateRange(customerDiscounts);
                await _context.SaveChangesAsync();
                result.ID = customerDiscounts.FirstOrDefault().ID;
                foreach (var customerDiscount in customerDiscounts)
                {
                    _context.Entry(customerDiscount).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> RemoveCustomerDiscountsAsync(List<CustomerDiscount> customerDiscounts)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.CustomerDiscounts.RemoveRange(customerDiscounts);
                await _context.SaveChangesAsync();
                result.ID = customerDiscounts.FirstOrDefault().ID;
                foreach (var customerDiscount in customerDiscounts)
                {
                    _context.Entry(customerDiscount).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> RemoveCustomerDiscountsAsync(List<long> customerDiscountIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var customerDiscountsToRemove = new List<CustomerDiscount>();

                foreach (var customerDiscountId in customerDiscountIds)
                {
                    var customerDiscount = await GetCustomerDiscountByIdAsync(customerDiscountId);
                    if (customerDiscount.Result != null)
                    {
                        customerDiscountsToRemove.Add(customerDiscount.Result);
                    }
                }

                if (customerDiscountsToRemove.Any())
                {
                    result = await RemoveCustomerDiscountsAsync(customerDiscountsToRemove);
                }
                else
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching customer discounts found to remove.";
                }
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
                result.ID = CustomerDiscountId;
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
                        (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Customer.Person.FirstName.ToString()) && x.Customer.Person.FirstName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Customer.Person.LastName.ToString()) && x.Customer.Person.LastName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText)) ||
                        (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                        (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
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

       
    }
}
