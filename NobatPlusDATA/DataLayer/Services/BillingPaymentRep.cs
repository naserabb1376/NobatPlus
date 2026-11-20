using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingPaymentRep : BillingReadRep<BillingPayment>, IBillingPaymentRep
{
    public BillingPaymentRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBillingPaymentAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BillingPayment>> GetAllBillingPaymentsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BillingPayment>> GetBillingPaymentByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

