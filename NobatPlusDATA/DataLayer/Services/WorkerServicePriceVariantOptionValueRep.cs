using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class WorkerServicePriceVariantOptionValueRep : IWorkerServicePriceVariantOptionValueRep
    {
        private readonly NobatPlusContext _context;

        public WorkerServicePriceVariantOptionValueRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddWorkerServicePriceVariantOptionValuesAsync(List<WorkerServicePriceVariantOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                optionValues = NormalizeRows(optionValues);
                if (!optionValues.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No valid worker service price variant option values were sent.";
                    return result;
                }

                var validationError = await ValidateRowsAsync(optionValues);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                var variantIds = optionValues.Select(x => x.WorkerServicePriceVariantID).Distinct().ToList();
                var optionValueIds = optionValues.Select(x => x.ServiceOptionValueID).Distinct().ToList();
                var existingRows = await _context.WorkerServicePriceVariantOptionValues
                    .Where(x => variantIds.Contains(x.WorkerServicePriceVariantID) &&
                                optionValueIds.Contains(x.ServiceOptionValueID))
                    .Select(x => new { x.WorkerServicePriceVariantID, x.ServiceOptionValueID })
                    .ToListAsync();

                var existingKeys = existingRows
                    .Select(x => (x.WorkerServicePriceVariantID, x.ServiceOptionValueID))
                    .ToHashSet();

                var rowsToInsert = optionValues
                    .Where(x => !existingKeys.Contains((x.WorkerServicePriceVariantID, x.ServiceOptionValueID)))
                    .ToList();

                if (rowsToInsert.Any())
                {
                    await _context.WorkerServicePriceVariantOptionValues.AddRangeAsync(rowsToInsert);
                }

                await SyncVariantCombinationKeysAsync(variantIds, optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.WorkerServicePriceVariantID ?? 0;
                foreach (var item in rowsToInsert) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditWorkerServicePriceVariantOptionValuesAsync(List<WorkerServicePriceVariantOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                optionValues = NormalizeRows(optionValues);
                if (!optionValues.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No valid worker service price variant option values were sent.";
                    return result;
                }

                var validationError = await ValidateRowsAsync(optionValues);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                var variantIds = optionValues.Select(x => x.WorkerServicePriceVariantID).Distinct().ToList();
                var oldRows = await _context.WorkerServicePriceVariantOptionValues
                    .Where(x => variantIds.Contains(x.WorkerServicePriceVariantID))
                    .ToListAsync();

                _context.WorkerServicePriceVariantOptionValues.RemoveRange(oldRows);
                await _context.WorkerServicePriceVariantOptionValues.AddRangeAsync(optionValues);
                await SyncVariantCombinationKeysAsync(variantIds, optionValues, replaceExisting: true);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.WorkerServicePriceVariantID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveWorkerServicePriceVariantOptionValuesAsync(List<WorkerServicePriceVariantOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.WorkerServicePriceVariantOptionValues.RemoveRange(optionValues);
                await SyncVariantCombinationKeysAsync(optionValues.Select(x => x.WorkerServicePriceVariantID).Distinct().ToList(), removedRows: optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.WorkerServicePriceVariantID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveWorkerServicePriceVariantOptionValuesAsync(List<(long WorkerServicePriceVariantId, long ServiceOptionValueId)> ids)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var rows = new List<WorkerServicePriceVariantOptionValue>();
                foreach (var id in ids)
                {
                    var row = await GetWorkerServicePriceVariantOptionValueByIdAsync(id.WorkerServicePriceVariantId, id.ServiceOptionValueId);
                    if (row.Result != null) rows.Add(row.Result);
                }

                if (!rows.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching worker service price variant option values found to remove.";
                    return result;
                }

                result = await RemoveWorkerServicePriceVariantOptionValuesAsync(rows);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistWorkerServicePriceVariantOptionValueAsync(long workerServicePriceVariantId, long serviceOptionValueId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.WorkerServicePriceVariantOptionValues.AsNoTracking()
                    .AnyAsync(x => x.WorkerServicePriceVariantID == workerServicePriceVariantId && x.ServiceOptionValueID == serviceOptionValueId);
                result.ID = workerServicePriceVariantId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<WorkerServicePriceVariantOptionValue>> GetAllWorkerServicePriceVariantOptionValuesAsync(long workerServicePriceVariantId = 0, long serviceOptionValueId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<WorkerServicePriceVariantOptionValue> results = new ListResultObject<WorkerServicePriceVariantOptionValue>();
            try
            {
                var query = _context.WorkerServicePriceVariantOptionValues
                    .Include(x => x.WorkerServicePriceVariant)
                    .Include(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .AsNoTracking();

                if (workerServicePriceVariantId > 0) query = query.Where(x => x.WorkerServicePriceVariantID == workerServicePriceVariantId);
                if (serviceOptionValueId > 0) query = query.Where(x => x.ServiceOptionValueID == serviceOptionValueId);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.WorkerServicePriceVariant.Price.ToString().Contains(searchText) ||
                        x.ServiceOptionValue.ValueName.Contains(searchText) ||
                        x.ServiceOptionValue.ServiceOption.OptionName.Contains(searchText));
                }

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.WorkerServicePriceVariantID).SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<WorkerServicePriceVariantOptionValue>> GetWorkerServicePriceVariantOptionValueByIdAsync(long workerServicePriceVariantId, long serviceOptionValueId)
        {
            RowResultObject<WorkerServicePriceVariantOptionValue> result = new RowResultObject<WorkerServicePriceVariantOptionValue>();
            try
            {
                result.Result = await _context.WorkerServicePriceVariantOptionValues
                    .Include(x => x.WorkerServicePriceVariant)
                    .Include(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.WorkerServicePriceVariantID == workerServicePriceVariantId && x.ServiceOptionValueID == serviceOptionValueId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        private static List<WorkerServicePriceVariantOptionValue> NormalizeRows(IEnumerable<WorkerServicePriceVariantOptionValue>? optionValues)
        {
            return optionValues?
                .Where(x => x.WorkerServicePriceVariantID > 0 && x.ServiceOptionValueID > 0)
                .GroupBy(x => new { x.WorkerServicePriceVariantID, x.ServiceOptionValueID })
                .Select(x => x.First())
                .ToList() ?? new List<WorkerServicePriceVariantOptionValue>();
        }

        private async Task<string> ValidateRowsAsync(List<WorkerServicePriceVariantOptionValue> optionValues)
        {
            var variantIds = optionValues.Select(x => x.WorkerServicePriceVariantID).Distinct().ToList();
            var variants = await _context.WorkerServicePriceVariants
                .AsNoTracking()
                .Where(x => variantIds.Contains(x.ID))
                .Select(x => new { x.ID, x.ServiceManagementID })
                .ToListAsync();

            if (variants.Count != variantIds.Count)
            {
                return "یک یا چند قیمت متغیر خدمت معتبر نیست";
            }

            var optionValueIds = optionValues.Select(x => x.ServiceOptionValueID).Distinct().ToList();
            var optionRows = await _context.ServiceOptionValues
                .AsNoTracking()
                .Include(x => x.ServiceOption)
                .Where(x => optionValueIds.Contains(x.ID))
                .Select(x => new
                {
                    x.ID,
                    x.ServiceOptionID,
                    x.ServiceOption.ServiceManagementID
                })
                .ToListAsync();

            if (optionRows.Count != optionValueIds.Count)
            {
                return "یک یا چند مقدار گزینه انتخاب شده معتبر نیست";
            }

            foreach (var group in optionValues.GroupBy(x => x.WorkerServicePriceVariantID))
            {
                var variant = variants.First(x => x.ID == group.Key);
                if (!await _context.ServiceManagements.AsNoTracking()
                        .AnyAsync(x => x.ID == variant.ServiceManagementID))
                {
                    return "خدمت انتخاب شده معتبر نیست";
                }

                var selectedOptionRows = optionRows
                    .Where(x => group.Select(g => g.ServiceOptionValueID).Contains(x.ID))
                    .ToList();

                if (selectedOptionRows.Any(x => x.ServiceManagementID != variant.ServiceManagementID))
                {
                    return "گزینه‌های انتخاب شده باید متعلق به همان خدمت باشند";
                }

                if (selectedOptionRows.GroupBy(x => x.ServiceOptionID).Any(x => x.Count() > 1))
                {
                    return "برای هر ویژگی فقط یک مقدار قابل انتخاب است";
                }
            }

            return "";
        }

        private async Task SyncVariantCombinationKeysAsync(
            List<long> variantIds,
            List<WorkerServicePriceVariantOptionValue>? addedRows = null,
            List<WorkerServicePriceVariantOptionValue>? removedRows = null,
            bool replaceExisting = false)
        {
            var variants = await _context.WorkerServicePriceVariants
                .Where(x => variantIds.Contains(x.ID))
                .ToListAsync();

            var existingRows = replaceExisting
                ? new List<WorkerServicePriceVariantOptionValue>()
                : await _context.WorkerServicePriceVariantOptionValues
                    .AsNoTracking()
                    .Where(x => variantIds.Contains(x.WorkerServicePriceVariantID))
                    .ToListAsync();

            addedRows ??= new List<WorkerServicePriceVariantOptionValue>();
            removedRows ??= new List<WorkerServicePriceVariantOptionValue>();

            foreach (var variant in variants)
            {
                var optionValueIds = existingRows
                    .Where(x => x.WorkerServicePriceVariantID == variant.ID)
                    .Select(x => x.ServiceOptionValueID)
                    .Concat(addedRows
                        .Where(x => x.WorkerServicePriceVariantID == variant.ID)
                        .Select(x => x.ServiceOptionValueID))
                    .Except(removedRows
                        .Where(x => x.WorkerServicePriceVariantID == variant.ID)
                        .Select(x => x.ServiceOptionValueID))
                    .Distinct()
                    .ToList();

                variant.OptionValueCombinationKey = WorkerServicePriceVariant.BuildOptionValueCombinationKey(optionValueIds);
            }
        }

        private async Task<long> GetRootServiceManagementIdAsync(long serviceManagementId)
        {
            var visitedIds = new HashSet<long>();
            var current = await _context.ServiceManagements
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID == serviceManagementId);

            while (current != null && current.ServiceParentID > 0 && visitedIds.Add(current.ID))
            {
                current = await _context.ServiceManagements
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ID == current.ServiceParentID);
            }

            return current?.ID ?? 0;
        }
    }
}
