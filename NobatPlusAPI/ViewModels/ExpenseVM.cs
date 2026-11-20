using Domains;

namespace NobatPlusDATA.ViewModels;

public class ExpenseVM : BaseEntity
{
    public long OwnerWorkerID { get; set; }
    public string OwnerWorkerName { get; set; } = "";
    public long ExpenseCategoryID { get; set; }
    public string ExpenseCategoryName { get; set; } = "";
    public string Title { get; set; } = "";
    public long AmountIRR { get; set; }
    public DateTime ExpenseDate { get; set; }
    public long CreatedByPersonID { get; set; }
}
