using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IJobTypeRep
    {
        public Task<ListResultObject<JobType>> GetAllJobTypesAsync(int SexTypeChecked = 0, int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<JobType>> GetJobTypeByIdAsync(long JobTypeId);
        public Task<BitResultObject> AddJobTypeAsync(JobType JobType);
        public Task<BitResultObject> EditJobTypeAsync(JobType JobType);
        public Task<BitResultObject> RemoveJobTypeAsync(JobType JobType);
        public Task<BitResultObject> RemoveJobTypeAsync(long JobTypeId);
        public Task<BitResultObject> ExistJobTypeAsync(long JobTypeId);
    }
}
