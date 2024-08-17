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
        public Task<List<Log>> GetAllLogsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<Log> GetLogByIdAsync(long LogId);
        public Task AddLogAsync(Log Log);
        public Task EditLogAsync(Log Log);
        public Task RemoveLogAsync(Log Log);
        public Task RemoveLogAsync(long LogId);
        public Task<bool> ExistLogAsync(long LogId);
    }
}
