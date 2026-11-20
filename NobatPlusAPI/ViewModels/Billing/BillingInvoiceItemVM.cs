using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BillingInvoiceItemVM : BaseEntity
{
    public long BillingInvoiceID { get; set; }
    public long BookingPlatformFeeID { get; set; }
    public decimal Amount { get; set; }
}

