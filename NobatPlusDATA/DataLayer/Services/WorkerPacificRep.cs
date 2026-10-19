using Domain;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class WorkerPacificRep : IWorkerPacificRep
    {

        private NobatPlusContext _context;
        public WorkerPacificRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddWorkerPacificAsync(WorkerPacific WorkerPacific)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var validationError = ValidateLeavePeriod(WorkerPacific);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                var overlappingLeave = await _context.WorkerPacifics
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkerID == WorkerPacific.WorkerID &&
                        x.PacificStartDate < WorkerPacific.PacificEndDate &&
                        x.PacificEndDate > WorkerPacific.PacificStartDate)
                    .OrderBy(x => x.PacificStartDate)
                    .FirstOrDefaultAsync();

                if (overlappingLeave != null)
                {
                    result.Status = false;
                    result.ErrorMessage = BuildOverlapErrorMessage(overlappingLeave);
                    return result;
                }

                await _context.WorkerPacifics.AddAsync(WorkerPacific);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.ID = WorkerPacific.ID;
                _context.Entry(WorkerPacific).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditWorkerPacificAsync(WorkerPacific WorkerPacific)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var validationError = ValidateLeavePeriod(WorkerPacific);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                var overlappingLeave = await _context.WorkerPacifics
                    .AsNoTracking()
                    .Where(x =>
                        x.ID != WorkerPacific.ID &&
                        x.WorkerID == WorkerPacific.WorkerID &&
                        x.PacificStartDate < WorkerPacific.PacificEndDate &&
                        x.PacificEndDate > WorkerPacific.PacificStartDate)
                    .OrderBy(x => x.PacificStartDate)
                    .FirstOrDefaultAsync();

                if (overlappingLeave != null)
                {
                    result.Status = false;
                    result.ErrorMessage = BuildOverlapErrorMessage(overlappingLeave);
                    return result;
                }

                _context.WorkerPacifics.Update(WorkerPacific);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                result.ID = WorkerPacific.ID;
                _context.Entry(WorkerPacific).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> ExistWorkerPacificAsync(long WorkerPacificId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.WorkerPacifics.AsNoTracking().AnyAsync(x => x.ID == WorkerPacificId);
                result.ID = WorkerPacificId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
             
        }

        public async Task<ListResultObject<WorkerPacific>> GetAllWorkerPacificsAsync(long workerId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<WorkerPacific> results = new ListResultObject<WorkerPacific>();
            try
            {
                IQueryable<WorkerPacific> query;

                query = _context.WorkerPacifics
                        .AsNoTracking()
                        .Include(x => x.Worker).ThenInclude(x => x.Person)
                        .Where(x =>
                            (!string.IsNullOrEmpty(x.Worker.Person.FirstName.ToString()) && x.Worker.Person.FirstName.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.Worker.Person.LastName.ToString()) && x.Worker.Person.LastName.ToString().Contains(searchText))                           
                            || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
                            || (x.PacificStartDate != null && x.PacificStartDate.ToString().Contains(searchText))
                            || (!string.IsNullOrEmpty(x.PacificEndDate.ToString()) && x.PacificEndDate.ToString().Contains(searchText))
                            || (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText))
                            || (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
                        );
                if (workerId > 0)
                {
                    query = query.Where(x=> x.WorkerID == workerId);
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

        public async Task<RowResultObject<WorkerPacific>> GetWorkerPacificByIdAsync(long WorkerPacificId)
        {
            RowResultObject<WorkerPacific> result = new RowResultObject<WorkerPacific>();
            try
            {
                result.Result = await _context.WorkerPacifics
                .AsNoTracking()
                .Include(x => x.Worker).ThenInclude(x => x.Person)
                .SingleOrDefaultAsync(x => x.ID == WorkerPacificId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveWorkerPacificAsync(WorkerPacific WorkerPacific)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.WorkerPacifics.Remove(WorkerPacific);
                await _context.SaveChangesAsync();
                result.ID = WorkerPacific.ID;
                _context.Entry(WorkerPacific).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveWorkerPacificAsync(long WorkerPacificId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var WorkerPacific = await GetWorkerPacificByIdAsync(WorkerPacificId);
                result = await RemoveWorkerPacificAsync(WorkerPacific.Result);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
          
        }

        private static string BuildOverlapErrorMessage(WorkerPacific overlappingLeave)
        {
            var start = overlappingLeave.PacificStartDate.ToShamsiString();
            var end = overlappingLeave.PacificEndDate.ToShamsiString();
            return $"برای این خدمات دهنده قبلاً از {start} تا {end} مرخصی ثبت شده است و بازه جدید با آن هم‌پوشانی دارد.".ToPersianDigits();
        }

        private static string ValidateLeavePeriod(WorkerPacific leave)
        {
            if (leave.PacificEndDate <= leave.PacificStartDate)
                return "زمان پایان مرخصی باید بعد از زمان شروع باشد.";

            var now = DateTime.Now.ToShamsi();
            var startIsPast = leave.PacificStartDate < now;
            var endIsPast = leave.PacificEndDate < now;

            if (startIsPast && endIsPast)
                return "زمان شروع و پایان مرخصی نمی‌تواند در گذشته باشد.";

            if (startIsPast)
                return "زمان شروع مرخصی نمی‌تواند در گذشته باشد.";

            if (endIsPast)
                return "زمان پایان مرخصی نمی‌تواند در گذشته باشد.";

            return string.Empty;
        }
    }
}
