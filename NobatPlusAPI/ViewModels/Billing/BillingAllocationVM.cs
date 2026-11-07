using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BillingAllocationVM : BaseEntity
{
    public long BillingPaymentID { get; set; }
    public long BookingPlatformFeeID { get; set; }
    public decimal Amount { get; set; }
    public DateTime AllocatedAt { get; set; }
    public DateTime? ReversedAt { get; set; }
}

