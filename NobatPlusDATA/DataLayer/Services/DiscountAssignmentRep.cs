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
    public class DiscountAssignmentRep : IDiscountAssignmentRep
    {

        private NobatPlusContext _context;
        public DiscountAssignmentRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddDiscountAssignmentAsync(DiscountAssignment DiscountAssignment)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.DiscountAssignments.AddAsync(DiscountAssignment);
                await _context.SaveChangesAsync();
                _context.Entry(DiscountAssignment).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditDiscountAssignmentAsync(DiscountAssignment DiscountAssignment)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.DiscountAssignments.Update(DiscountAssignment);
                await _context.SaveChangesAsync();
                _context.Entry(DiscountAssignment).State = EntityState.Detached;
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
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<DiscountAssignment>> GetAllDiscountAssignmentsAsync(long DiscountId, long AdminId = 0, long StylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
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
                            (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Admin.Person.FirstName) && x.Admin.Person.FirstName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Admin.Person.LastName) && x.Admin.Person.LastName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)))
                        );
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
                             (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Admin.Person.FirstName) && x.Admin.Person.FirstName.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Admin.Person.LastName) && x.Admin.Person.LastName.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                             (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)))
                         );
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
                            (!string.IsNullOrEmpty(x.Discount.StartDate.ToString()) && x.Discount.StartDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Discount.EndDate.ToString()) && x.Discount.EndDate.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Admin.Person.FirstName) && x.Admin.Person.FirstName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Admin.Person.LastName) && x.Admin.Person.LastName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)))
                        );
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

        public async Task<RowResultObject<DiscountAssignment>> GetDiscountAssignmentByIdAsync(long DiscountAssignmentId)
        {
            RowResultObject<DiscountAssignment> result = new RowResultObject<DiscountAssignment>();
            try
            {
                result.Result = await _context.DiscountAssignments
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == DiscountAssignmentId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveDiscountAssignmentAsync(DiscountAssignment DiscountAssignment)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.DiscountAssignments.Remove(DiscountAssignment);
                await _context.SaveChangesAsync();
                _context.Entry(DiscountAssignment).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveDiscountAssignmentAsync(long DiscountAssignmentId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var DiscountAssignment = await GetDiscountAssignmentByIdAsync(DiscountAssignmentId);
                result = await RemoveDiscountAssignmentAsync(DiscountAssignment.Result);
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
