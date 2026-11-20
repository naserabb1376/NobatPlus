using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingInvoiceRep : BillingReadRep<BillingInvoice>, IBillingInvoiceRep
{
    public BillingInvoiceRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBillingInvoiceAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BillingInvoice>> GetAllBillingInvoicesAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BillingInvoice>> GetBillingInvoiceByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

