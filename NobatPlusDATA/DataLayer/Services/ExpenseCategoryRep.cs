using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services;

public class ExpenseCategoryRep : IExpenseCategoryRep
{
    private readonly NobatPlusContext _context;
    public ExpenseCategoryRep(NobatPlusContext context) => _context = context;

    private IQueryable<ExpenseCategory> Scope(long ownerWorkerId, long personId, bool isAdmin, bool includeInactive)
    {
        var query = _context.ExpenseCategories.AsQueryable();
        if (!isAdmin) query = query.Where(x => x.OwnerWorkerID == null || x.OwnerWorkerID == ownerWorkerId ||
            (x.OwnerWorkerID.HasValue && _context.Workers.Any(w => w.ID == x.OwnerWorkerID.Value && w.PersonID == personId)));
        if (!includeInactive) query = query.Where(x => x.IsActive);
        return query.Include(x => x.OwnerWorker);
    }

    public async Task<ListResultObject<ExpenseCategory>> GetAllExpenseCategoriesAsync(long ownerWorkerId, long personId, bool isAdmin, bool includeInactive = false, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
    {
        var result = new ListResultObject<ExpenseCategory>();
        try
        {
            var query = Scope(ownerWorkerId, personId, isAdmin, includeInactive);
            if (!string.IsNullOrWhiteSpace(searchText)) query = query.Where(x => x.Name.Contains(searchText));
            result.TotalCount = await query.CountAsync(); result.PageCount = DbTools.GetPageCount(result.TotalCount, pageSize);
            result.Results = await query.OrderBy(x => x.Name).SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
        }
        catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
        return result;
    }

    public async Task<RowResultObject<ExpenseCategory>> GetExpenseCategoryByIdAsync(long id, long personId, bool isAdmin)
    {
        var result = new RowResultObject<ExpenseCategory>();
        try { result.Result = await Scope(0, personId, isAdmin, true).SingleOrDefaultAsync(x => x.ID == id); if (result.Result == null) { result.Status = false; result.ErrorMessage = "دسته‌بندی یافت نشد یا دسترسی مجاز نیست"; } }
        catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
        return result;
    }

    public async Task<BitResultObject> ExistExpenseCategoryAsync(long id, long personId, bool isAdmin)
    { var result = new BitResultObject { ID = id }; try { result.Status = id > 0 && await Scope(0, personId, isAdmin, true).AnyAsync(x => x.ID == id); } catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; } return result; }

    public async Task<BitResultObject> AddExpenseCategoryAsync(ExpenseCategory category, long personId, bool isAdmin)
    { var result = new BitResultObject(); try { var error = await ValidateAsync(category, personId, isAdmin); if (error != null) return Fail(result, error); category.Name = category.Name.Trim(); category.CreatedByPersonID = personId; await _context.ExpenseCategories.AddAsync(category); await _context.SaveChangesAsync(); result.ID = category.ID; } catch (DbUpdateException) { return Fail(result, "دسته‌بندی دیگری با همین نام برای این مالک وجود دارد"); } catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); } return result; }

    public async Task<BitResultObject> EditExpenseCategoryAsync(ExpenseCategory category, long personId, bool isAdmin)
    { var result = new BitResultObject { ID = category.ID }; try { var row = await _context.ExpenseCategories.SingleOrDefaultAsync(x => x.ID == category.ID); if (row == null) return Fail(result, "دسته‌بندی یافت نشد"); var error = await ValidateAsync(category, personId, isAdmin, category.ID); if (error != null) return Fail(result, error); row.Name = category.Name.Trim(); row.IsActive = category.IsActive; row.Description = category.Description; row.UpdateDate = category.UpdateDate; await _context.SaveChangesAsync(); } catch (DbUpdateException) { return Fail(result, "دسته‌بندی دیگری با همین نام برای این مالک وجود دارد"); } catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); } return result; }

    public async Task<BitResultObject> RemoveExpenseCategoryAsync(long id, long personId, bool isAdmin)
    { var result = new BitResultObject { ID = id }; try { var row = await _context.ExpenseCategories.SingleOrDefaultAsync(x => x.ID == id); if (row == null) return Fail(result, "دسته‌بندی یافت نشد"); if (!isAdmin && row.OwnerWorkerID != null && !await OwnsAsync(row.OwnerWorkerID.Value, personId)) return Fail(result, "دسترسی حذف این دسته‌بندی مجاز نیست"); if (await _context.Expenses.AnyAsync(x => x.ExpenseCategoryID == id)) return Fail(result, "این دسته‌بندی در هزینه‌ها استفاده شده و قابل حذف نیست؛ آن را غیرفعال کنید"); _context.ExpenseCategories.Remove(row); await _context.SaveChangesAsync(); } catch (Exception ex) { return Fail(result, $"{ex.Message} - {ex.InnerException?.Message}"); } return result; }

    private async Task<string?> ValidateAsync(ExpenseCategory category, long personId, bool isAdmin, long exceptId = 0)
    { if (string.IsNullOrWhiteSpace(category.Name)) return "نام دسته‌بندی الزامی است"; if (category.OwnerWorkerID == null && !isAdmin) return "فقط مدیر می‌تواند دسته‌بندی عمومی ایجاد کند"; if (category.OwnerWorkerID.HasValue && !isAdmin && !await OwnsAsync(category.OwnerWorkerID.Value, personId)) return "مالک دسته‌بندی متعلق به شما نیست"; var name = category.Name.Trim(); if (await _context.ExpenseCategories.AnyAsync(x => x.ID != exceptId && x.OwnerWorkerID == category.OwnerWorkerID && x.Name == name)) return "دسته‌بندی دیگری با همین نام وجود دارد"; return null; }
    private Task<bool> OwnsAsync(long workerId, long personId) => _context.Workers.AnyAsync(x => x.ID == workerId && x.PersonID == personId);
    private static BitResultObject Fail(BitResultObject result, string message) { result.Status = false; result.ErrorMessage = message; return result; }
}
