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

        public void AddLog(Log Log)
        {
            _context.Logs.Add(Log);
            _context.SaveChanges();
            _context.Entry(Log).State = EntityState.Detached;
        }

        public void EditLog(Log Log)
        {
            _context.Logs.Update(Log);
            _context.SaveChanges();
            _context.Entry(Log).State = EntityState.Detached;
        }

        public bool ExistLog(long LogId)
        {
            return _context.Logs.Any(x => x.ID == LogId);
        }

        public List<Log> GetAllLogs(int pageIndex = 1, int pageSize = 20, string searchText = "")
        {
            return _context.Logs.Where(x =>
            (!string.IsNullOrEmpty(x.ActionName.ToString()) && x.ActionName.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.Description.ToString()) && x.Description.ToString().Contains(searchText))
           || (!string.IsNullOrEmpty(x.LogTime.ToString()) && x.LogTime.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.CreateDate.ToString()) && x.CreateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
           || (!string.IsNullOrEmpty(x.UpdateDate.ToString()) && x.UpdateDate.Value.ToString("yyyy/MM/dd HH:mm").Contains(searchText))
            ).OrderByDescending(x => x.CreateDate).ToPaging(pageIndex,pageSize).ToList();
        }

        public Log GetLogById(long LogId)
        {
            return _context.Logs.Find(LogId);
        }

        public void RemoveLog(Log Log)
        {
            _context.Logs.Remove(Log);
            _context.SaveChanges();
            _context.Entry(Log).State = EntityState.Detached;
        }

        public void RemoveLog(long LogId)
        {
            var Log = GetLogById(LogId);
            RemoveLog(Log);
        }
    }
}
