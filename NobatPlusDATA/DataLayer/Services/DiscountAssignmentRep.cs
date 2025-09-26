using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
    public class DiscountAssignmentRep : IDiscountAssignmentRep
    {

        private NobatPlusContext _context;
        public DiscountAssignmentRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddDiscountAssignmentsAsync(List<DiscountAssignment> DiscountAssignments)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.DiscountAssignments.AddRangeAsync(DiscountAssignments);
                await _context.SaveChangesAsync();
                result.ID = DiscountAssignments.FirstOrDefault().ID;
                foreach (var discountAssignment in DiscountAssignments)
                {
                    _context.Entry(discountAssignment).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> EditDiscountAssignmentsAsync(List<DiscountAssignment> DiscountAssignments)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.DiscountAssignments.UpdateRange(DiscountAssignments);
                await _context.SaveChangesAsync();
                result.ID = DiscountAssignments.FirstOrDefault().ID;
                foreach (var discountAssignment in DiscountAssignments)
                {
                    _context.Entry(discountAssignment).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }


        public async Task<BitResultObject> RemoveDiscountAssignmentsAsync(List<DiscountAssignment> discountAssignments)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.DiscountAssignments.RemoveRange(discountAssignments);
                await _context.SaveChangesAsync();
                result.ID = discountAssignments.FirstOrDefault().ID;
                foreach (var discountAssignment in discountAssignments)
                {
                    _context.Entry(discountAssignment).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveDiscountAssignmentsAsync(List<long> discountAssignmentIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var discountAssignmentsToRemove = new List<DiscountAssignment>();

                foreach (var discountAssignmentId in discountAssignmentIds)
                {
                    var discountAssignment = await GetDiscountAssignmentByIdAsync(discountAssignmentId);
                    if (discountAssignment.Result != null)
                    {
                        discountAssignmentsToRemove.Add(discountAssignment.Result);
                    }
                }

                if (discountAssignmentsToRemove.Any())
                {
                    result = await RemoveDiscountAssignmentsAsync(discountAssignmentsToRemove);
                }
                else
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching discount assignments found to remove.";
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }



        public async Task<BitResultObject> ExistDiscountAssignmentAsync(long DiscountAssignmentId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.DiscountAssignments
                .AsNoTracking()
                .AnyAsync(x => x.ID == DiscountAssignmentId);
                result.ID = DiscountAssignmentId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<DiscountAssignment>> GetAllDiscountAssignmentsAsync(long DiscountId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<DiscountAssignment> results = new ListResultObject<DiscountAssignment>();
            try
            {
                IQueryable<DiscountAssignment> query;

                if (AdminId > 0)
                {
                    query = _context.DiscountAssignments
                        .AsNoTracking()
                        .Include(x => x.Discount)
                        .Include(x => x.Admin).ThenInclude(x => x.Person)
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Where(x =>
                            x.DiscountId == DiscountId && x.AdminId == AdminId &&
                            ((!string.IsNullOrEmpty(x.Discount.DiscountCode) && x.Discount.DiscountCode.Contains(searchText)) ||
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
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        ));
                }

                if (StylistId > 0)
                {
                    query = _context.DiscountAssignments
                         .AsNoTracking()
                         .Include(x => x.Discount)
                         .Include(x => x.Admin).ThenInclude(x => x.Person)
                         .Include(x => x.Stylist).ThenInclude(x => x.Person)
                         .Where(x =>
                             x.DiscountId == DiscountId && x.StylistId == StylistId &&
                             ((!string.IsNullOrEmpty(x.Discount.DiscountCode) && x.Discount.DiscountCode.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountAmount.ToString().Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Discount.Description) && x.Discount.Description.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString().Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString().Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Admin.Person.FirstName) && x.Admin.Person.FirstName.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Admin.Person.LastName) && x.Admin.Person.LastName.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                             (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                             (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                         ));
                }
                else
                {
                    query = _context.DiscountAssignments
                        .AsNoTracking()
                        .Include(x => x.Discount)
                        .Include(x => x.Admin).ThenInclude(x => x.Person)
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Where(x =>
                            x.DiscountId == DiscountId &&
                            ((!string.IsNullOrEmpty(x.Discount.DiscountCode) && x.Discount.DiscountCode.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Discount.DiscountAmount.ToString()) && x.Discount.DiscountAmount.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Discount.Description) && x.Discount.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Admin.Person.FirstName) && x.Admin.Person.FirstName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Admin.Person.LastName) && x.Admin.Person.LastName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        ));
                }

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

        public async Task<RowResultObject<DiscountAssignment>> GetDiscountAssignmentByIdAsync(long DiscountAssignmentId)
        {
            RowResultObject<DiscountAssignment> result = new RowResultObject<DiscountAssignment>();
            try
            {
                result.Result = await _context.DiscountAssignments
                .AsNoTracking()
                        .Include(x => x.Discount)
                        .Include(x => x.Admin).ThenInclude(x => x.Person)
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .SingleOrDefaultAsync(x => x.ID == DiscountAssignmentId);
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
