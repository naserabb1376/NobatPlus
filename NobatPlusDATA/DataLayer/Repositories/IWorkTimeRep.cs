using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWorkTimeRep
    {
        public Task<ListResultObject<WorkTime>> GetAllWorkTimesAsync(long stylistId=0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<WorkTime>> GetWorkTimeByIdAsync(long WorkTimeId);
        public Task<BitResultObject> AddWorkTimeAsync(WorkTime WorkTime);
        public Task<BitResultObject> EditWorkTimeAsync(WorkTime WorkTime);
        public Task<BitResultObject> RemoveWorkTimeAsync(WorkTime WorkTime);
        public Task<BitResultObject> RemoveWorkTimeAsync(long WorkTimeId);
        public Task<BitResultObject> ExistWorkTimeAsync(long WorkTimeId);
    }
}
