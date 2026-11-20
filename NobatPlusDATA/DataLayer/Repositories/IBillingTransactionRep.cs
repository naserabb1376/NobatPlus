using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBillingTransactionRep
{
    Task<BitResultObject> ExistBillingTransactionAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BillingTransaction>> GetAllBillingTransactionsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BillingTransaction>> GetBillingTransactionByIdAsync(long id, long personId, bool isAdmin);
}

