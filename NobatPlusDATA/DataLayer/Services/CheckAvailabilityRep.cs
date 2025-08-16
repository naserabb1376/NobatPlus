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
    public class CheckAvailabilityRep : ICheckAvailabilityRep
    {

        private NobatPlusContext _context;
        public CheckAvailabilityRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddCheckAvailabilityAsync(CheckAvailability CheckAvailability)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.CheckAvailabilities.AddAsync(CheckAvailability);
                await _context.SaveChangesAsync();
                result.ID = CheckAvailability.ID;
                _context.Entry(CheckAvailability).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditCheckAvailabilityAsync(CheckAvailability CheckAvailability)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.CheckAvailabilities.Update(CheckAvailability);
                await _context.SaveChangesAsync();
                result.ID = CheckAvailability.ID;
                _context.Entry(CheckAvailability).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistCheckAvailabilityAsync(long CheckAvailabilityId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.CheckAvailabilities
                .AsNoTracking()
                .AnyAsync(x => x.ID == CheckAvailabilityId);
                result.ID = CheckAvailabilityId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<CheckAvailability>> GetAllCheckAvailabilitiesAsync(long stylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<CheckAvailability> results = new ListResultObject<CheckAvailability>();
            try
            {
                IQueryable<CheckAvailability> query;

                if (stylistId == 0)
                {
                    query = _context.CheckAvailabilities
                        .AsNoTracking()
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Time.ToString()) && x.Time.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                            (!string.IsNullOrEmpty(x.Date.ToString()) && x.Date.ToString().Contains(searchText)) ||
                            (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                            (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        );
                }
                else
                {
                    query = _context.CheckAvailabilities
                        .AsNoTracking()
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Where(x =>
                            x.StylistID == stylistId &&
                            (
                                (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Stylist.Specialty.ToString()) && x.Stylist.Specialty.ToString().Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Time.ToString()) && x.Time.ToString().Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                                (!string.IsNullOrEmpty(x.Date.ToString()) && x.Date.ToString().Contains(searchText)) ||
                                (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                                (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                            )
                        );
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

        public async Task<RowResultObject<CheckAvailability>> GetCheckAvailabilityByIdAsync(long CheckAvailabilityId)
        {
            RowResultObject<CheckAvailability> result = new RowResultObject<CheckAvailability>();
            try
            {
                result.Result = await _context.CheckAvailabilities
                .AsNoTracking()
 .SingleOrDefaultAsync(x => x.ID == CheckAvailabilityId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveCheckAvailabilityAsync(CheckAvailability CheckAvailability)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.CheckAvailabilities.Remove(CheckAvailability);
                await _context.SaveChangesAsync();
                result.ID = CheckAvailability.ID;
                _context.Entry(CheckAvailability).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveCheckAvailabilityAsync(long CheckAvailabilityId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var CheckAvailability = await GetCheckAvailabilityByIdAsync(CheckAvailabilityId);
                result = await RemoveCheckAvailabilityAsync(CheckAvailability.Result);
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
