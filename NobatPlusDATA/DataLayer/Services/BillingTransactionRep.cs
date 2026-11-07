using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingTransactionRep : BillingReadRep<BillingTransaction>, IBillingTransactionRep
{
    public BillingTransactionRep(NobatPlusContext context) : base(context) { }
}

