using Domains;

namespace NobatPlusDATA.ViewModels;

public class ExpenseCategoryVM : BaseEntity
{
    public string Name { get; set; } = "";
    public long? OwnerWorkerID { get; set; }
    public string OwnerWorkerName { get; set; } = "";
    public long? CreatedByPersonID { get; set; }
}
