using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBillingAllocationRep
{
    Task<BitResultObject> ExistBillingAllocationAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BillingAllocation>> GetAllBillingAllocationsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BillingAllocation>> GetBillingAllocationByIdAsync(long id, long personId, bool isAdmin);
}

