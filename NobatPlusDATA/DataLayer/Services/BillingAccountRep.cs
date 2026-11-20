using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingAccountRep : BillingReadRep<BillingAccount>, IBillingAccountRep
{
    public BillingAccountRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBillingAccountAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BillingAccount>> GetAllBillingAccountsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BillingAccount>> GetBillingAccountByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

