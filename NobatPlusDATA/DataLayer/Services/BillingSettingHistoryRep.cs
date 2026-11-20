using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingSettingHistoryRep : BillingReadRep<BillingSettingHistory>, IBillingSettingHistoryRep
{
    public BillingSettingHistoryRep(NobatPlusContext context) : base(context) { }
}

