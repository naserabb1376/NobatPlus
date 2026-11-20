using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ExpenseCategory;

public class GetExpenseCategoryListRequestBody : GetListRequestBody
{
    public long OwnerWorkerID { get; set; }
    public bool IncludeInactive { get; set; }
}

public class AddEditExpenseCategoryRequestBody
{
    public long ID { get; set; }
    public long? OwnerWorkerID { get; set; }
    [Required, StringLength(150)] public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
