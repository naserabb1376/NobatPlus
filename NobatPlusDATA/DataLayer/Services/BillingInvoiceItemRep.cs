using NobatPlusDATA.Domain;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Services;
public class BillingInvoiceItemRep : BillingReadRep<BillingInvoiceItem>, IBillingInvoiceItemRep
{
    public BillingInvoiceItemRep(NobatPlusContext context) : base(context) { }
    public Task<BitResultObject> ExistBillingInvoiceItemAsync(long id, long personId, bool isAdmin) => ExistAsync(id, personId, isAdmin);
    public Task<ListResultObject<BillingInvoiceItem>> GetAllBillingInvoiceItemsAsync(BillingListFilter filter, long personId, bool isAdmin) => GetAllAsync(filter, personId, isAdmin);
    public Task<RowResultObject<BillingInvoiceItem>> GetBillingInvoiceItemByIdAsync(long id, long personId, bool isAdmin) => GetByIdAsync(id, personId, isAdmin);
}

