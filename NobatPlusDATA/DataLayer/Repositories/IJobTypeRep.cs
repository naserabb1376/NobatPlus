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
        public List<JobType> GetAllJobTypes(int SexTypeChecked = 0, int pageIndex = 1,int pageSize = 20, string searchText ="");
        public JobType GetJobTypeById(long JobTypeId);
        public void AddJobType(JobType JobType);
        public void EditJobType(JobType JobType);
        public void RemoveJobType(JobType JobType);
        public void RemoveJobType(long JobTypeId);
        public bool ExistJobType(long JobTypeId);
    }
}
