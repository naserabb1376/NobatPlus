using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BusinessBillingSettingVM : BaseEntity
{
    public long BusinessID { get; set; }
    public string DefaultFeePayer { get; set; } = "Business";
    public string CollectionMode { get; set; } = "Hybrid";
    public string InvoiceCycle { get; set; } = "Monthly";
    public string SettlementDestination { get; set; } = "Business";
    public bool IsBookingFeeEnabled { get; set; } = true;
    public decimal CreditLimit { get; set; }
    public int DueDays { get; set; } = 7;
    public DateTime EffectiveFrom { get; set; }
}

