using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingAccountRep : BillingReadRep<BillingAccount>, IBillingAccountRep
{
    public BillingAccountRep(NobatPlusContext context) : base(context) { }
}

