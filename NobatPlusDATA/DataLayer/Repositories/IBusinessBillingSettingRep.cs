using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBusinessBillingSettingRep
{
    Task<BitResultObject> ExistBusinessBillingSettingAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BusinessBillingSetting>> GetAllBusinessBillingSettingsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BusinessBillingSetting>> GetBusinessBillingSettingByIdAsync(long id, long personId, bool isAdmin);
}

