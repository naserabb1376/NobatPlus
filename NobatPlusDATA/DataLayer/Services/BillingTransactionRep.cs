using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingTransactionRep : BillingReadRep<BillingTransaction>, IBillingTransactionRep
{
    public BillingTransactionRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBillingTransactionAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BillingTransaction>> GetAllBillingTransactionsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BillingTransaction>> GetBillingTransactionByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

