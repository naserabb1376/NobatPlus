using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBusinessSubscriptionRep
{
    Task<BitResultObject> ExistBusinessSubscriptionAsync(long businessId);
    Task<ListResultObject<BusinessSubscription>> GetAllBusinessSubscriptionsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BusinessSubscription>> GetBusinessSubscriptionByIdAsync(long id, long personId, bool isAdmin);
}

