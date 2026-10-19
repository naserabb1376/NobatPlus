using Domain;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWorkerRep
    {
        public Task<ListResultObject<WorkerDTO>> GetAllWorkersAsync(long parentId = 0, List<long> serviceIds = null, long jobTypeId = 0, long discountId = 0, decimal fromPrice = 0, decimal toPrice = 0, long cityId = 0, int gender = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "", FindLocationRequestBody findLocation = null, string accountStatus = "");
        public Task<RowResultObject<WorkerDTO>> GetWorkerByIdAsync(long WorkerId);
        public Task<ListResultObject<WorkerProfileDTO>> GetWorkerProfilesByPersonIdAsync(long personId);
        public Task<BitResultObject> AddWorkerAsync(Worker Worker);
        public Task<BitResultObject> EditWorkerAsync(Worker Worker);
        public Task<BitResultObject> RemoveWorkerAsync(Worker Worker);
        public Task<BitResultObject> RemoveWorkerAsync(long WorkerId);
        public Task<BitResultObject> ExistWorkerAsync(string fieldValue, string fieldName);
    }
}
