using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BusinessBillingSettingRep : BillingReadRep<BusinessBillingSetting>, IBusinessBillingSettingRep
{
    public BusinessBillingSettingRep(NobatPlusContext context) : base(context) { }
}

