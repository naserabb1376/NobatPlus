using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingRefundRep : BillingReadRep<BillingRefund>, IBillingRefundRep
{
    public BillingRefundRep(NobatPlusContext context) : base(context) { }
}

