using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BusinessBillingSettingRep : BillingReadRep<BusinessBillingSetting>, IBusinessBillingSettingRep
{
    public BusinessBillingSettingRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBusinessBillingSettingAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BusinessBillingSetting>> GetAllBusinessBillingSettingsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BusinessBillingSetting>> GetBusinessBillingSettingByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

