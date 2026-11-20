using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBillingAccountRep
{
    Task<BitResultObject> ExistBillingAccountAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BillingAccount>> GetAllBillingAccountsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BillingAccount>> GetBillingAccountByIdAsync(long id, long personId, bool isAdmin);
}

