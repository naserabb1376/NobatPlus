using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class WorkerBillingSettingVM : BaseEntity
{
    public long BusinessID { get; set; }
    public long WorkerID { get; set; }
    public string? FeePayerOverride { get; set; }
    public decimal RevenueSharePercent { get; set; }
    public long? AcceptedByPersonID { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptedContractVersion { get; set; }
}

