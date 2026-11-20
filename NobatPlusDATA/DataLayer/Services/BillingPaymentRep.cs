using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingPaymentRep : BillingReadRep<BillingPayment>, IBillingPaymentRep
{
    public BillingPaymentRep(NobatPlusContext context) : base(context) { }
}

