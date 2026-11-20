using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class SubscriptionPlanRep : BillingReadRep<SubscriptionPlan>, ISubscriptionPlanRep
{
    public SubscriptionPlanRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistSubscriptionPlanAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<SubscriptionPlan>> GetAllSubscriptionPlansAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<SubscriptionPlan>> GetSubscriptionPlanByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

