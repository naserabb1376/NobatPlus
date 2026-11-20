using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BillingInvoiceVM : BaseEntity
{
    public decimal RemainingAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal ReversedAmount { get; set; }
    public long BillingAccountID { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "open";
    public string CurrencyCode { get; set; } = "IRR";
    public decimal TotalAmount { get; set; }
}

