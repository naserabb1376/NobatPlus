using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBillingInvoiceItemRep
{
    Task<BitResultObject> ExistBillingInvoiceItemAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BillingInvoiceItem>> GetAllBillingInvoiceItemsAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BillingInvoiceItem>> GetBillingInvoiceItemByIdAsync(long id, long personId, bool isAdmin);
}

