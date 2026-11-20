using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingAllocationRep : BillingReadRep<BillingAllocation>, IBillingAllocationRep
{
    public BillingAllocationRep(NobatPlusContext context) : base(context) { }
}

