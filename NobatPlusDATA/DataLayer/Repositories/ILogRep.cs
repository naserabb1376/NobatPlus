using Domains;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ILogRep
    {
        public Task<ListResultObject<Log>> GetAllLogsAsync(int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<Log>> GetLogByIdAsync(long LogId);
        public Task<BitResultObject> AddLogAsync(Log Log);
        public Task<BitResultObject> EditLogAsync(Log Log);
        public Task<BitResultObject> RemoveLogAsync(Log Log);
        public Task<BitResultObject> RemoveLogAsync(long LogId);
        public Task<BitResultObject> ExistLogAsync(long LogId);
    }
}
