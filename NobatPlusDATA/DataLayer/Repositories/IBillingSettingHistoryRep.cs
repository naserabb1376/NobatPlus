using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBillingSettingHistoryRep
{
    Task<BitResultObject> ExistBillingSettingHistoryAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BillingSettingHistory>> GetAllBillingSettingHistoriesAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BillingSettingHistory>> GetBillingSettingHistoryByIdAsync(long id, long personId, bool isAdmin);
}

