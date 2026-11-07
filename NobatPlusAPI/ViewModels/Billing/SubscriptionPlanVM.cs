using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class SubscriptionPlanVM : BaseEntity
{
    public string Name { get; set; } = "";
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public decimal BookingFeeAmount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string ContractVersion { get; set; } = "1";
}

