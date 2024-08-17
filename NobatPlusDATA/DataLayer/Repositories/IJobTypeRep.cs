using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IJobTypeRep
    {
        public Task<List<JobType>> GetAllJobTypesAsync(int SexTypeChecked = 0, int pageIndex = 1, int pageSize = 20, string searchText = "");
        public Task<JobType> GetJobTypeByIdAsync(long JobTypeId);
        public Task AddJobTypeAsync(JobType JobType);
        public Task EditJobTypeAsync(JobType JobType);
        public Task RemoveJobTypeAsync(JobType JobType);
        public Task RemoveJobTypeAsync(long JobTypeId);
        public Task<bool> ExistJobTypeAsync(long JobTypeId);
    }
}
