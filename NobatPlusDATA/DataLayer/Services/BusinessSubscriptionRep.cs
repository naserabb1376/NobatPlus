using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
using Microsoft.EntityFrameworkCore;
namespace NobatPlusDATA.DataLayer.Services;
public class BusinessSubscriptionRep : BillingReadRep<BusinessSubscription>, IBusinessSubscriptionRep
{
    public BusinessSubscriptionRep(NobatPlusContext context) : base(context) { }

    public Task<ListResultObject<BusinessSubscription>> GetAllBusinessSubscriptionsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BusinessSubscription>> GetBusinessSubscriptionByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);

    public async Task<BitResultObject> ExistBusinessSubscriptionAsync(long businessId)
    {
        var result = new BitResultObject { ID = businessId };
        try
        {
            if (businessId <= 0)
            {
                result.Status = false;
                result.ErrorMessage = "شناسه مجموعه معتبر نیست";
                return result;
            }

            var now = DateTime.UtcNow;
            result.Status = await Context.BusinessSubscriptions
                .AsNoTracking()
                .AnyAsync(x => x.BusinessID == businessId && x.Status == "active" &&
                    x.StartsAt <= now && x.EndsAt > now);
        }
        catch (Exception ex)
        {
            result.Status = false;
            result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
        }

        return result;
    }
}

