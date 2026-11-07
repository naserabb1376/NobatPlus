using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BillingAccountVM : BaseEntity
{
    public long WorkerID { get; set; }
    public string OwnerType { get; set; } = "Worker";
    public string CurrencyCode { get; set; } = "IRR";
}

