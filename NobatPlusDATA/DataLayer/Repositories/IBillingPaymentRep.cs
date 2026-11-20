using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBillingPaymentRep
{
    Task<BitResultObject> ExistBillingPaymentAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BillingPayment>> GetAllBillingPaymentsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BillingPayment>> GetBillingPaymentByIdAsync(long id, long personId, bool isAdmin);
}

