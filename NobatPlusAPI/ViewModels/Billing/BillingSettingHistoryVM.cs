using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BillingSettingHistoryVM : BaseEntity
{
    public long BusinessID { get; set; }
    public long? WorkerID { get; set; }
    public long ActorPersonID { get; set; }
    public string BeforeJson { get; set; } = "";
    public string AfterJson { get; set; } = "";
    public DateTime OccurredAt { get; set; }
}

