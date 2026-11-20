using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BusinessSubscriptionVM : BaseEntity
{
    public long BusinessID { get; set; }
    public long SubscriptionPlanID { get; set; }
    public long PaymentID { get; set; }
    public Guid RequestID { get; set; }
    public string PlanNameSnapshot { get; set; } = "";
    public string PlanTypeSnapshot { get; set; } = "Individual";
    public int MaxMemberCountSnapshot { get; set; } = 1;
    public int DurationMonthsSnapshot { get; set; }
    public decimal PriceSnapshot { get; set; }
    public decimal BookingFeeAmountSnapshot { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string ContractVersionSnapshot { get; set; } = "";
    public string Status { get; set; } = "pending";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}

