using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingSettingHistoryRep : BillingReadRep<BillingSettingHistory>, IBillingSettingHistoryRep
{
    public BillingSettingHistoryRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBillingSettingHistoryAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BillingSettingHistory>> GetAllBillingSettingHistoriesAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BillingSettingHistory>> GetBillingSettingHistoryByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

