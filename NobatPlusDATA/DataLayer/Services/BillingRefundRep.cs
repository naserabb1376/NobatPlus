using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingRefundRep : BillingReadRep<BillingRefund>, IBillingRefundRep
{
    public BillingRefundRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBillingRefundAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BillingRefund>> GetAllBillingRefundsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BillingRefund>> GetBillingRefundByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

