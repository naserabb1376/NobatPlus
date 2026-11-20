using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class SubscriptionPlanVM : BaseEntity
{
    public string Name { get; set; } = "";
    public string PlanType { get; set; } = "Individual";
    public int MaxMemberCount { get; set; } = 1;
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public decimal BookingFeeAmount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string ContractVersion { get; set; } = "1";
}

