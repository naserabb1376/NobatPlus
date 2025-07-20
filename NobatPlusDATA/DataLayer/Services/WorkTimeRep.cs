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
    public class WorkTimeRep : IWorkTimeRep
    {

        private NobatPlusContext _context;
        public WorkTimeRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddWorkTimeAsync(WorkTime WorkTime)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.WorkTimes.AddAsync(WorkTime);
                await _context.SaveChangesAsync();
                result.ID = WorkTime.ID;
                _context.Entry(WorkTime).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditWorkTimeAsync(WorkTime WorkTime)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.WorkTimes.Update(WorkTime);
                await _context.SaveChangesAsync();
                result.ID = WorkTime.ID;
                _context.Entry(WorkTime).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistWorkTimeAsync(long WorkTimeId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.WorkTimes.AsNoTracking().AnyAsync(x => x.ID == WorkTimeId);
                result.ID = WorkTimeId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
             
        }

        public async Task<ListResultObject<WorkTime>> GetAllWorkTimesAsync(long stylistId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<WorkTime> results = new ListResultObject<WorkTime>();
            try
            {
                IQueryable<WorkTime> query;

                query = _context.WorkTimes
                        .AsNoTracking()
                        .Include(x => x.Stylist).ThenInclude(x => x.Person)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.Stylist.Person.FirstName.ToString()) && x.Stylist.Person.FirstName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Stylist.Person.LastName.ToString()) && x.Stylist.Person.LastName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.DayOfWeek.ToString()) && x.DayOfWeek.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                            || (x.WorkStartTime != null && x.WorkStartTime.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.WorkEndTime.ToString()) && x.WorkEndTime.ToString().Contains(searchText))
                            || (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText))
                            || (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        );
                if (stylistId > 0)
                {
                    query = query.Where(x=> x.StylistID == stylistId);
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

        public async Task<RowResultObject<WorkTime>> GetWorkTimeByIdAsync(long WorkTimeId)
        {
            RowResultObject<WorkTime> result = new RowResultObject<WorkTime>();
            try
            {
                result.Result = await _context.WorkTimes
                .AsNoTracking()
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .SingleOrDefaultAsync(x => x.ID == WorkTimeId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveWorkTimeAsync(WorkTime WorkTime)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.WorkTimes.Remove(WorkTime);
                await _context.SaveChangesAsync();
                result.ID = WorkTime.ID;
                _context.Entry(WorkTime).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveWorkTimeAsync(long WorkTimeId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var WorkTime = await GetWorkTimeByIdAsync(WorkTimeId);
                result = await RemoveWorkTimeAsync(WorkTime.Result);
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
