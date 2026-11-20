using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories;

public interface IExpenseCategoryRep
{
    Task<ListResultObject<ExpenseCategory>> GetAllExpenseCategoriesAsync(long ownerWorkerId, long personId, bool isAdmin, bool includeInactive = false, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
    Task<RowResultObject<ExpenseCategory>> GetExpenseCategoryByIdAsync(long id, long personId, bool isAdmin);
    Task<BitResultObject> ExistExpenseCategoryAsync(long id, long personId, bool isAdmin);
    Task<BitResultObject> AddExpenseCategoryAsync(ExpenseCategory category, long personId, bool isAdmin);
    Task<BitResultObject> EditExpenseCategoryAsync(ExpenseCategory category, long personId, bool isAdmin);
    Task<BitResultObject> RemoveExpenseCategoryAsync(long id, long personId, bool isAdmin);
}
