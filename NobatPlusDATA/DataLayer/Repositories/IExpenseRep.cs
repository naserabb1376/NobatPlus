using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories;

public interface IExpenseRep
{
    Task<ListResultObject<Expense>> GetAllExpensesAsync(long ownerWorkerId, long personId, bool isAdmin, long categoryId = 0, DateTime? fromDate = null, DateTime? toDate = null, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
    Task<RowResultObject<Expense>> GetExpenseByIdAsync(long id, long personId, bool isAdmin);
    Task<BitResultObject> ExistExpenseAsync(long id, long personId, bool isAdmin);
    Task<BitResultObject> AddExpenseAsync(Expense expense, long personId, bool isAdmin);
    Task<BitResultObject> EditExpenseAsync(Expense expense, long personId, bool isAdmin);
    Task<BitResultObject> RemoveExpenseAsync(long id, long personId, bool isAdmin);
}
