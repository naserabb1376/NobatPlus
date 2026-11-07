using Domains;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services;

public abstract class BillingReadRep<T> : IBillingReadRep<T> where T : BaseEntity
{
    protected readonly NobatPlusContext Context;
    protected BillingReadRep(NobatPlusContext context) => Context = context;

    private IQueryable<T> Scoped(long personId, bool admin)
    {
        var query = Context.Set<T>().AsNoTracking();
        if (admin) return query;
        var owned = Context.Workers.Where(x => x.PersonID == personId).Select(x => x.ID);
        var accounts = Context.BillingAccounts.Where(x => owned.Contains(x.WorkerID)).Select(x => x.ID);
        if (typeof(T) == typeof(SubscriptionPlan)) return query.Where(x => x.IsActive);
        if (typeof(T) == typeof(BillingAccount)) return query.Where(x => owned.Contains(EF.Property<long>(x, "WorkerID")));
        if (typeof(T) == typeof(BusinessSubscription) || typeof(T) == typeof(BusinessBillingSetting) || typeof(T) == typeof(BillingSettingHistory))
            return query.Where(x => owned.Contains(EF.Property<long>(x, "BusinessID")));
        if (typeof(T) == typeof(WorkerBillingSetting))
            return query.Where(x => owned.Contains(EF.Property<long>(x, "BusinessID")) || owned.Contains(EF.Property<long>(x, "WorkerID")));
        if (typeof(T) == typeof(BillingInvoiceItem))
            return query.Where(x => Context.BillingInvoices.Any(i => i.ID == EF.Property<long>(x, "BillingInvoiceID") && accounts.Contains(i.BillingAccountID)));
        if (typeof(T) == typeof(BillingAllocation) || typeof(T) == typeof(BillingRefund))
            return query.Where(x => Context.BillingPayments.Any(p => p.ID == EF.Property<long>(x, "BillingPaymentID") && accounts.Contains(p.BillingAccountID)));
        return query.Where(x => accounts.Contains(EF.Property<long>(x, "BillingAccountID")));
    }

    public async Task<ListResultObject<T>> GetAllAsync(BillingListFilter filter, long personId, bool isAdmin)
    {
        var result = new ListResultObject<T>();
        try
        {
            if (filter.PageIndex < 1 || filter.PageSize < 0 || filter.PageSize > 500)
                throw new InvalidOperationException("مقادیر صفحه‌بندی معتبر نیست");
            var query = Scoped(personId, isAdmin);
            foreach (var property in typeof(BillingListFilter).GetProperties().Where(x => x.PropertyType == typeof(long)))
            {
                var value = (long)property.GetValue(filter)!;
                if (value <= 0) continue;
                var target = typeof(T).GetProperty(property.Name);
                if (target == null) throw new InvalidOperationException($"فیلتر {property.Name} برای این جدول معتبر نیست");
                if (target.PropertyType == typeof(long?)) query = query.Where(x => EF.Property<long?>(x, property.Name) == value);
                else query = query.Where(x => EF.Property<long>(x, property.Name) == value);
            }
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                if (typeof(T).GetProperty("Status") == null) throw new InvalidOperationException("این جدول فیلتر وضعیت ندارد");
                query = query.Where(x => EF.Property<string>(x, "Status") == filter.Status);
            }
            if (filter.FromDate.HasValue) query = query.Where(x => x.CreateDate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue) query = query.Where(x => x.CreateDate < filter.ToDate.Value);
            if (!string.IsNullOrWhiteSpace(filter.SearchText)) query = query.Where(x => x.Description != null && x.Description.Contains(filter.SearchText));
            result.TotalCount = await query.CountAsync();
            result.PageCount = DbTools.GetPageCount(result.TotalCount, filter.PageSize);
            result.Results = await query.OrderByDescending(x => x.ID).SortBy(filter.SortQuery).ToPaging(filter.PageIndex, filter.PageSize).ToListAsync();
            await FillInvoiceBalancesAsync(result.Results);
        }
        catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
        return result;
    }

    public async Task<RowResultObject<T>> GetByIdAsync(long id, long personId, bool isAdmin)
    {
        var result = new RowResultObject<T>();
        try
        {
            result.Result = await Scoped(personId, isAdmin).SingleOrDefaultAsync(x => x.ID == id);
            if (result.Result == null) { result.Status = false; result.ErrorMessage = "رکورد یافت نشد یا دسترسی مجاز نیست"; }
            else await FillInvoiceBalancesAsync(new[] { result.Result });
        }
        catch (Exception ex) { result.Status = false; result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}"; }
        return result;
    }

    private async Task FillInvoiceBalancesAsync(IEnumerable<T> rows)
    {
        if (typeof(T) != typeof(BillingInvoice)) return;
        foreach (var invoice in rows.Cast<BillingInvoice>())
        {
            var items = Context.BillingInvoiceItems.Where(x => x.BillingInvoiceID == invoice.ID);
            invoice.ReversedAmount = await (from i in items join f in Context.BookingPlatformFees on i.BookingPlatformFeeID equals f.ID
                                             where f.Status == "reversed" select (decimal?)i.Amount).SumAsync() ?? 0;
            var feeIds = items.Select(x => x.BookingPlatformFeeID);
            var activeTotal = await Context.BookingPlatformFees.Where(x => feeIds.Contains(x.ID) && x.Status == "accrued").SumAsync(x => x.FeeAmount);
            var activeAllocated = await Context.BillingAllocations.Where(x => feeIds.Contains(x.BookingPlatformFeeID) && x.ReversedAt == null).SumAsync(x => x.Amount);
            invoice.RemainingAmount = Math.Max(0, activeTotal - activeAllocated);
            invoice.AllocatedAmount = Math.Max(0, invoice.TotalAmount - invoice.ReversedAmount - invoice.RemainingAmount);
        }
    }
}
