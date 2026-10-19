using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWorkerScheduleBlockRep
    {
        Task<ListResultObject<WorkerScheduleBlockDTO>> GetAllWorkerScheduleBlocksAsync(long workerId, DateTime? fromDate = null, DateTime? toDate = null, long serviceManagementId = 0, long bookingTagId = 0, string status = "", long customerId = 0, long discountId = 0, int pageIndex = 1, int pageSize = 100, string searchText = "", string sortQuery = "");
        Task<RowResultObject<WorkerScheduleBlockDTO>> GetWorkerScheduleBlockByIdAsync(long id, long customerId = 0, long discountId = 0);
        Task<BitResultObject> AddWorkerScheduleBlocksAsync(List<WorkerScheduleBlock> blocks);
        Task<BitResultObject> EditWorkerScheduleBlockAsync(WorkerScheduleBlock block);
        Task<BitResultObject> RemoveWorkerScheduleBlockAsync(long id);
    }
}
