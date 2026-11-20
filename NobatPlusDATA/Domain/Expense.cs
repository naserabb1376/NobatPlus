using Domains;

namespace NobatPlusDATA.Domain;

public class ExpenseCategory : BaseEntity
{
    public string Name { get; set; } = "";
    public long? OwnerWorkerID { get; set; }
    public long? CreatedByPersonID { get; set; }

    public Worker? OwnerWorker { get; set; }
    public Person? CreatedByPerson { get; set; }
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

public class Expense : BaseEntity
{
    public long OwnerWorkerID { get; set; }
    public long ExpenseCategoryID { get; set; }
    public string Title { get; set; } = "";
    public long AmountIRR { get; set; }
    public DateTime ExpenseDate { get; set; }
    public long CreatedByPersonID { get; set; }
    public Worker OwnerWorker { get; set; } = null!;
    public ExpenseCategory ExpenseCategory { get; set; } = null!;
    public Person CreatedByPerson { get; set; } = null!;
}
