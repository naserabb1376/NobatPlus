using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class WorkerBillingSettingRep : BillingReadRep<WorkerBillingSetting>, IWorkerBillingSettingRep
{
    public WorkerBillingSettingRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistWorkerBillingSettingAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<WorkerBillingSetting>> GetAllWorkerBillingSettingsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<WorkerBillingSetting>> GetWorkerBillingSettingByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

