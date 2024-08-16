using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ILogRep
    {
        public List<Log> GetAllLogs(int pageIndex = 1,int pageSize = 20, string searchText ="");
        public Log GetLogById(long LogId);
        public void AddLog(Log Log);
        public void EditLog(Log Log);
        public void RemoveLog(Log Log);
        public void RemoveLog(long LogId);
        public bool ExistLog(long LogId);
    }
}
