using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingInvoiceRep : BillingReadRep<BillingInvoice>, IBillingInvoiceRep
{
    public BillingInvoiceRep(NobatPlusContext context) : base(context) { }
}

