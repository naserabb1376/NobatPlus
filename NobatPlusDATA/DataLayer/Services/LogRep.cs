using Domains;
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
    public class LogRep : ILogRep
    {

        private NobatPlusContext _context;
        public LogRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddLogAsync(Log Log)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.Logs.AddAsync(Log);
                await _context.SaveChangesAsync();
                result.ID = Log.ID;
                _context.Entry(Log).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> EditLogAsync(Log Log)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Logs.Update(Log);
                await _context.SaveChangesAsync();
                result.ID = Log.ID;
                _context.Entry(Log).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> ExistLogAsync(long LogId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.Logs
                .AsNoTracking()
                .AnyAsync(x => x.ID == LogId);
                result.ID = LogId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<ListResultObject<Log>> GetAllLogsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            ListResultObject<Log> results = new ListResultObject<Log>();
            try
            {
                var query = _context.Logs
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.ActionName.ToString()) && x.ActionName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.LogTime.ToString()) && x.LogTime.ToString().Contains(searchText)) ||
                    (x.CreateDate.HasValue && x.CreateDate.Value.ToString().Contains(searchText)) ||
                    (x.UpdateDate.HasValue && x.UpdateDate.Value.ToString().Contains(searchText))
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

        public async Task<RowResultObject<Log>> GetLogByIdAsync(long LogId)
        {
            RowResultObject<Log> result = new RowResultObject<Log>();
            try
            {
                result.Result = await _context.Logs
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == LogId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
            
        }

        public async Task<BitResultObject> RemoveLogAsync(Log Log)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.Logs.Remove(Log);
                await _context.SaveChangesAsync();
                result.ID = Log.ID;
                _context.Entry(Log).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
           
        }

        public async Task<BitResultObject> RemoveLogAsync(long LogId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var Log = await GetLogByIdAsync(LogId);
                result = await RemoveLogAsync(Log.Result);
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
