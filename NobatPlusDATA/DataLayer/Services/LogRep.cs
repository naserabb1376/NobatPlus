using Domains;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
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

        public async Task AddLogAsync(Log Log)
        {
            _context.Logs.Add(Log);
            await _context.SaveChangesAsync();
            _context.Entry(Log).State = EntityState.Detached;
        }

        public async Task EditLogAsync(Log Log)
        {
            _context.Logs.Update(Log);
            await _context.SaveChangesAsync();
            _context.Entry(Log).State = EntityState.Detached;
        }

        public async Task<bool> ExistLogAsync(long LogId)
        {
            return await _context.Logs
                .AsNoTracking()
                .AnyAsync(x => x.ID == LogId);
        }

        public async Task<List<Log>> GetAllLogsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return await _context.Logs
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.ActionName.ToString()) && x.ActionName.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.LogTime.ToString()) && x.LogTime.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
                )
                .OrderByDescending(x => x.CreateDate)
                .ToPaging(pageIndex, pageSize)
                .ToListAsync();
        }

        public async Task<Log> GetLogByIdAsync(long LogId)
        {
            return await _context.Logs
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.ID == LogId);
        }

        public async Task RemoveLogAsync(Log Log)
        {
            _context.Logs.Remove(Log);
            await _context.SaveChangesAsync();
            _context.Entry(Log).State = EntityState.Detached;
        }

        public async Task RemoveLogAsync(long LogId)
        {
            var Log = await GetLogByIdAsync(LogId);
            await RemoveLogAsync(Log);
        }
    }
}
