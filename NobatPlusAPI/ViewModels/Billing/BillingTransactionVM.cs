using Domains;
namespace NobatPlusAPI.ViewModels.Billing;
public class BillingTransactionVM : BaseEntity
{
    public long BillingAccountID { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "";
    public string SourceType { get; set; } = "";
    public long SourceID { get; set; }
    public DateTime OccurredAt { get; set; }
}

