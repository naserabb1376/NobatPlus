using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class SubscriptionPlanRep : BillingReadRep<SubscriptionPlan>, ISubscriptionPlanRep
{
    public SubscriptionPlanRep(NobatPlusContext context) : base(context) { }
}

