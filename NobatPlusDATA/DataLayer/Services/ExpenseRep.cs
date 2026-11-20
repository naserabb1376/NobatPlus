using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services;

public class ExpenseRep : IExpenseRep
{
    private readonly NobatPlusContext _context;
    public ExpenseRep(NobatPlusContext context) => _context = context;

    private IQueryable<Expense> Scope(long ownerWorkerId, long personId, bool isAdmin)
    {
        var query = _context.Expenses.AsQueryable();
        if (!isAdmin) query = query.Where(x => x.OwnerWorkerID == ownerWorkerId ||
            _context.Workers.Any(w => w.ID == x.OwnerWorkerID && w.PersonID == personId));
        return query.Include(x => x.OwnerWorker).Include(x => x.ExpenseCategory);
    }

    public async Task<ListResultObject<Expense>> GetAllExpensesAsync(long ownerWorkerId, long personId, bool isAdmin, long categoryId = 0, DateTime? fromDate = null, DateTime? toDate = null, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
    {
        var result = new ListResultObject<Expense>();
        try
        {
            var query = Scope(ownerWorkerId, personId, isAdmin);
            if (categoryId > 0) query = query.Where(x => x.ExpenseCategoryID == categoryId);
            if (fromDate.HasValue) query = query.Where(x => x.ExpenseDate >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(x => x.ExpenseDate < toDate.Value.Date.AddDays(1));
            if (!string.IsNullOrWhiteSpace(searchText)) query = query.Where(x => x.Title.Contains(searchText) || (x.Description != null && x.Description.Contains(searchText)) || x.ExpenseCategory.Name.Contains(searchText));
            result.TotalCount = await query.CountAsync(); result.PageCount = DbTools.GetPageCount(result.TotalCount, pageSize);
            result.Results = await query.OrderByDescending(x => x.ExpenseDate).ThenByDescending(x => x.ID).SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
        }
        catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
        return result;
    }

    public async Task<RowResultObject<Expense>> GetExpenseByIdAsync(long id, long personId, bool isAdmin)
    { var result = new RowResultObject<Expense>(); try { result.Result = await Scope(0, personId, isAdmin).SingleOrDefaultAsync(x => x.ID == id); if (result.Result == null) { result.Status = false; result.ErrorMessage = "هزینه یافت نشد یا دسترسی مجاز نیست"; } } catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; } return result; }

    public async Task<BitResultObject> ExistExpenseAsync(long id, long personId, bool isAdmin)
    { var result = new BitResultObject { ID = id }; try { result.Status = id > 0 && await Scope(0, personId, isAdmin).AnyAsync(x => x.ID == id); } catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; } return result; }

    public async Task<BitResultObject> AddExpenseAsync(Expense expense, long personId, bool isAdmin)
    { var result = new BitResultObject(); try { var error = await ValidateAsync(expense, personId, isAdmin); if (error != null) return Fail(result, error); expense.CreatedByPersonID = personId; await _context.Expenses.AddAsync(expense); await _context.SaveChangesAsync(); result.ID = expense.ID; } catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); } return result; }

    public async Task<BitResultObject> EditExpenseAsync(Expense expense, long personId, bool isAdmin)
    { var result = new BitResultObject { ID = expense.ID }; try { var row = await _context.Expenses.SingleOrDefaultAsync(x => x.ID == expense.ID); if (row == null) return Fail(result, "هزینه یافت نشد"); var error = await ValidateAsync(expense, personId, isAdmin); if (error != null) return Fail(result, error); row.OwnerWorkerID = expense.OwnerWorkerID; row.ExpenseCategoryID = expense.ExpenseCategoryID; row.Title = expense.Title.Trim(); row.AmountIRR = expense.AmountIRR; row.ExpenseDate = expense.ExpenseDate.Date; row.Description = expense.Description; row.UpdateDate = expense.UpdateDate; await _context.SaveChangesAsync(); } catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); } return result; }

    public async Task<BitResultObject> RemoveExpenseAsync(long id, long personId, bool isAdmin)
    { var result = new BitResultObject { ID = id }; try { var row = await _context.Expenses.SingleOrDefaultAsync(x => x.ID == id); if (row == null) return Fail(result, "هزینه یافت نشد"); if (!isAdmin && !await OwnsAsync(row.OwnerWorkerID, personId)) return Fail(result, "دسترسی حذف این هزینه مجاز نیست"); _context.Expenses.Remove(row); await _context.SaveChangesAsync(); } catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); } return result; }

    private async Task<string?> ValidateAsync(Expense expense, long personId, bool isAdmin)
    { if (string.IsNullOrWhiteSpace(expense.Title)) return "عنوان هزینه الزامی است"; if (expense.AmountIRR <= 0) return "مبلغ هزینه باید بیشتر از صفر باشد"; if (!isAdmin && !await OwnsAsync(expense.OwnerWorkerID, personId)) return "مالک هزینه متعلق به شما نیست"; var ownerExists = await _context.Workers.AnyAsync(x => x.ID == expense.OwnerWorkerID); if (!ownerExists) return "مالک هزینه یافت نشد"; var category = await _context.ExpenseCategories.AsNoTracking().SingleOrDefaultAsync(x => x.ID == expense.ExpenseCategoryID && x.IsActive && (x.OwnerWorkerID == null || x.OwnerWorkerID == expense.OwnerWorkerID)); if (category == null) return "دسته‌بندی برای این مالک معتبر نیست"; return null; }
    private Task<bool> OwnsAsync(long workerId, long personId) => _context.Workers.AnyAsync(x => x.ID == workerId && x.PersonID == personId);
    private static BitResultObject Fail(BitResultObject result, string message) { result.Status = false; result.ErrorMessage = message; return result; }
}
