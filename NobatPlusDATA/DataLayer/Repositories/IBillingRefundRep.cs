using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBillingRefundRep
{
    Task<BitResultObject> ExistBillingRefundAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BillingRefund>> GetAllBillingRefundsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BillingRefund>> GetBillingRefundByIdAsync(long id, long personId, bool isAdmin);
}

