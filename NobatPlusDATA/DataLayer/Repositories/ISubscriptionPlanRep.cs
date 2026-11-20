using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface ISubscriptionPlanRep
{
    Task<BitResultObject> ExistSubscriptionPlanAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<SubscriptionPlan>> GetAllSubscriptionPlansAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<SubscriptionPlan>> GetSubscriptionPlanByIdAsync(long id, long personId, bool isAdmin);
}

