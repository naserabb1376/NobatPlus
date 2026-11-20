using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
namespace NobatPlusDATA.DataLayer.Repositories;
public interface IBillingInvoiceRep
{
    Task<BitResultObject> ExistBillingInvoiceAsync(long id, long personId, bool isAdmin);
    Task<ListResultObject<BillingInvoice>> GetAllBillingInvoicesAsync(BillingListFilter filter, long personId, bool isAdmin);
    Task<RowResultObject<BillingInvoice>> GetBillingInvoiceByIdAsync(long id, long personId, bool isAdmin);
}

