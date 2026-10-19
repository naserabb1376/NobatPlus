using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWorkerServicePriceVariantRep
    {
        Task<ListResultObject<WorkerServicePriceVariant>> GetAllWorkerServicePriceVariantsAsync(long workerId = 0, long serviceManagementId = 0, int isActive = -1, bool onlyLeafServices = false, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<WorkerServicePriceVariant>> GetWorkerServicePriceVariantByIdAsync(long workerServicePriceVariantId);
        Task<BitResultObject> AddWorkerServicePriceVariantAsync(WorkerServicePriceVariant workerServicePriceVariant);
        Task<BitResultObject> EditWorkerServicePriceVariantAsync(WorkerServicePriceVariant workerServicePriceVariant);
        Task<BitResultObject> RemoveWorkerServicePriceVariantAsync(long workerServicePriceVariantId);
        Task<BitResultObject> ExistWorkerServicePriceVariantAsync(long workerServicePriceVariantId);
    }
}
