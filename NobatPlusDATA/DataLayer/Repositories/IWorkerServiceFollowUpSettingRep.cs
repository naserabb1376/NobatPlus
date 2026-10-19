using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWorkerServiceFollowUpSettingRep
    {
        Task<ListResultObject<WorkerServiceFollowUpSetting>> GetAllWorkerServiceFollowUpSettingsAsync(long workerId = 0, long serviceManagementId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<WorkerServiceFollowUpSetting>> GetWorkerServiceFollowUpSettingByIdAsync(long settingId);
        Task<List<WorkerServiceFollowUpSetting>> GetActiveWorkerServiceFollowUpSettingsAsync(long workerId, List<long> serviceManagementIds);
        Task<BitResultObject> AddWorkerServiceFollowUpSettingAsync(WorkerServiceFollowUpSetting setting);
        Task<BitResultObject> EditWorkerServiceFollowUpSettingAsync(WorkerServiceFollowUpSetting setting);
        Task<BitResultObject> RemoveWorkerServiceFollowUpSettingAsync(long settingId);
        Task<BitResultObject> ExistWorkerServiceFollowUpSettingAsync(long settingId);
    }
}
