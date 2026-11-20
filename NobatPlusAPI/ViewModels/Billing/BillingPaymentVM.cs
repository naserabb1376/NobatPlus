using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BillingPaymentVM : BaseEntity
{
    public long BillingAccountID { get; set; }
    public long PaymentID { get; set; }
    public long? BillingInvoiceID { get; set; }
    public Guid RequestID { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string Status { get; set; } = "pending";
    public DateTime? ConfirmedAt { get; set; }
}

