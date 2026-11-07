using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BusinessSubscriptionRep : BillingReadRep<BusinessSubscription>, IBusinessSubscriptionRep
{
    public BusinessSubscriptionRep(NobatPlusContext context) : base(context) { }
}

