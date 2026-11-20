using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Expense;

public class GetExpenseListRequestBody : GetListRequestBody
{
    public long OwnerWorkerID { get; set; }
    public long ExpenseCategoryID { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeDeleted { get; set; }
}

public class AddEditExpenseRequestBody
{
    public long ID { get; set; }
    [Range(1, long.MaxValue)] public long OwnerWorkerID { get; set; }
    [Range(1, long.MaxValue)] public long ExpenseCategoryID { get; set; }
    [Required, StringLength(200)] public string Title { get; set; } = "";
    [Range(1, long.MaxValue)] public long AmountIRR { get; set; }
    [Required] public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
}
