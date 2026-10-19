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
    public interface IWorkerPacificRep
    {
        public Task<ListResultObject<WorkerPacific>> GetAllWorkerPacificsAsync(long workerId=0,int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="");
        public Task<RowResultObject<WorkerPacific>> GetWorkerPacificByIdAsync(long WorkerPacificId);
        public Task<BitResultObject> AddWorkerPacificAsync(WorkerPacific WorkerPacific);
        public Task<BitResultObject> EditWorkerPacificAsync(WorkerPacific WorkerPacific);
        public Task<BitResultObject> RemoveWorkerPacificAsync(WorkerPacific WorkerPacific);
        public Task<BitResultObject> RemoveWorkerPacificAsync(long WorkerPacificId);
        public Task<BitResultObject> ExistWorkerPacificAsync(long WorkerPacificId);
    }
}
