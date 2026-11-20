using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingAllocationRep : BillingReadRep<BillingAllocation>, IBillingAllocationRep
{
    public BillingAllocationRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBillingAllocationAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BillingAllocation>> GetAllBillingAllocationsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BillingAllocation>> GetBillingAllocationByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

