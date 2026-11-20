using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IWorkerBillingSettingRep
{
    Task<BitResultObject> ExistWorkerBillingSettingAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<WorkerBillingSetting>> GetAllWorkerBillingSettingsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<WorkerBillingSetting>> GetWorkerBillingSettingByIdAsync(long id, long personId, bool isAdmin);
}

