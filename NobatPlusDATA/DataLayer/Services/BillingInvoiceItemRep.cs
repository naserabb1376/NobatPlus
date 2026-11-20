using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingInvoiceItemRep : BillingReadRep<BillingInvoiceItem>, IBillingInvoiceItemRep
{
    public BillingInvoiceItemRep(NobatPlusContext context) : base(context) { }
}

