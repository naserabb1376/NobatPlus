using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWorkerServicePriceVariantOptionValueRep
    {
        Task<ListResultObject<WorkerServicePriceVariantOptionValue>> GetAllWorkerServicePriceVariantOptionValuesAsync(long workerServicePriceVariantId = 0, long serviceOptionValueId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<WorkerServicePriceVariantOptionValue>> GetWorkerServicePriceVariantOptionValueByIdAsync(long workerServicePriceVariantId, long serviceOptionValueId);
        Task<BitResultObject> AddWorkerServicePriceVariantOptionValuesAsync(List<WorkerServicePriceVariantOptionValue> optionValues);
        Task<BitResultObject> EditWorkerServicePriceVariantOptionValuesAsync(List<WorkerServicePriceVariantOptionValue> optionValues);
        Task<BitResultObject> RemoveWorkerServicePriceVariantOptionValuesAsync(List<WorkerServicePriceVariantOptionValue> optionValues);
        Task<BitResultObject> RemoveWorkerServicePriceVariantOptionValuesAsync(List<(long WorkerServicePriceVariantId, long ServiceOptionValueId)> ids);
        Task<BitResultObject> ExistWorkerServicePriceVariantOptionValueAsync(long workerServicePriceVariantId, long serviceOptionValueId);
    }
}
