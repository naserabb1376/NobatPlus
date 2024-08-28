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
    public class JobTypeRep : IJobTypeRep
    {

        private NobatPlusContext _context;
        public JobTypeRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddJobTypeAsync(JobType JobType)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.JobTypes.AddAsync(JobType);
                await _context.SaveChangesAsync();
                result.ID = JobType.ID;
                _context.Entry(JobType).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> EditJobTypeAsync(JobType JobType)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.JobTypes.Update(JobType);
                await _context.SaveChangesAsync();
                result.ID = JobType.ID;
                _context.Entry(JobType).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistJobTypeAsync(long JobTypeId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.JobTypes
                .AsNoTracking()
                .AnyAsync(x => x.ID == JobTypeId);
                result.ID = JobTypeId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<JobType>> GetAllJobTypesAsync(int SexTypeChecked = 0, int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<JobType> results = new ListResultObject<JobType>();
            try
            {
                var query = _context.JobTypes
                .AsNoTracking()
                .Where(x =>
                    x.SexTypeChecked == SexTypeChecked &&
                    (
                        (!string.IsNullOrEmpty(x.JobTitle.ToString()) && x.JobTitle.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.SexTypeChecked.ToString()) && x.SexTypeChecked.ToString().Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
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

        public async Task<RowResultObject<JobType>> GetJobTypeByIdAsync(long JobTypeId)
        {
            RowResultObject<JobType> result = new RowResultObject<JobType>();
            try
            {
                result.Result = await _context.JobTypes
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == JobTypeId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveJobTypeAsync(JobType JobType)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.JobTypes.Remove(JobType);
                await _context.SaveChangesAsync();
                result.ID = JobType.ID;
                _context.Entry(JobType).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveJobTypeAsync(long JobTypeId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var JobType = await GetJobTypeByIdAsync(JobTypeId);
                result = await RemoveJobTypeAsync(JobType.Result);
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
