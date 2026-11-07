using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BillingRefundVM : BaseEntity
{
    public long BillingPaymentID { get; set; }
    public Guid RequestID { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public long RequestedByPersonID { get; set; }
    public long? ResolvedByPersonID { get; set; }
    public string? BankReference { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

