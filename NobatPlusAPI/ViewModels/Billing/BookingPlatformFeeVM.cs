using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BookingPlatformFeeVM : BaseEntity
{
    public long BookingID { get; set; }
    public long BusinessID { get; set; }
    public long WorkerID { get; set; }
    public long BillingAccountID { get; set; }
    public long BusinessSubscriptionID { get; set; }
    public decimal FeeAmount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string FeePayerSnapshot { get; set; } = "";
    public string CollectionModeSnapshot { get; set; } = "";
    public string ContractVersionSnapshot { get; set; } = "";
    public string Status { get; set; } = "accrued";
    public DateTime AccruedAt { get; set; }
    public DateTime? ReversedAt { get; set; }
}

