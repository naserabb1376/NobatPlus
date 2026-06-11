using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class StylistServicePriceVariantOptionValueRep : IStylistServicePriceVariantOptionValueRep
    {
        private readonly NobatPlusContext _context;

        public StylistServicePriceVariantOptionValueRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddStylistServicePriceVariantOptionValuesAsync(List<StylistServicePriceVariantOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                optionValues = NormalizeRows(optionValues);
                if (!optionValues.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No valid stylist service price variant option values were sent.";
                    return result;
                }

                var validationError = await ValidateRowsAsync(optionValues);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                var variantIds = optionValues.Select(x => x.StylistServicePriceVariantID).Distinct().ToList();
                var optionValueIds = optionValues.Select(x => x.ServiceOptionValueID).Distinct().ToList();
                var existingRows = await _context.StylistServicePriceVariantOptionValues
                    .Where(x => variantIds.Contains(x.StylistServicePriceVariantID) &&
                                optionValueIds.Contains(x.ServiceOptionValueID))
                    .Select(x => new { x.StylistServicePriceVariantID, x.ServiceOptionValueID })
                    .ToListAsync();

                var existingKeys = existingRows
                    .Select(x => (x.StylistServicePriceVariantID, x.ServiceOptionValueID))
                    .ToHashSet();

                var rowsToInsert = optionValues
                    .Where(x => !existingKeys.Contains((x.StylistServicePriceVariantID, x.ServiceOptionValueID)))
                    .ToList();

                if (rowsToInsert.Any())
                {
                    await _context.StylistServicePriceVariantOptionValues.AddRangeAsync(rowsToInsert);
                }

                await SyncVariantCombinationKeysAsync(variantIds, optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.StylistServicePriceVariantID ?? 0;
                foreach (var item in rowsToInsert) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditStylistServicePriceVariantOptionValuesAsync(List<StylistServicePriceVariantOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                optionValues = NormalizeRows(optionValues);
                if (!optionValues.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No valid stylist service price variant option values were sent.";
                    return result;
                }

                var validationError = await ValidateRowsAsync(optionValues);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                var variantIds = optionValues.Select(x => x.StylistServicePriceVariantID).Distinct().ToList();
                var oldRows = await _context.StylistServicePriceVariantOptionValues
                    .Where(x => variantIds.Contains(x.StylistServicePriceVariantID))
                    .ToListAsync();

                _context.StylistServicePriceVariantOptionValues.RemoveRange(oldRows);
                await _context.StylistServicePriceVariantOptionValues.AddRangeAsync(optionValues);
                await SyncVariantCombinationKeysAsync(variantIds, optionValues, replaceExisting: true);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.StylistServicePriceVariantID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveStylistServicePriceVariantOptionValuesAsync(List<StylistServicePriceVariantOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.StylistServicePriceVariantOptionValues.RemoveRange(optionValues);
                await SyncVariantCombinationKeysAsync(optionValues.Select(x => x.StylistServicePriceVariantID).Distinct().ToList(), removedRows: optionValues);
                await _context.SaveChangesAsync();
                result.ID = optionValues.FirstOrDefault()?.StylistServicePriceVariantID ?? 0;
                foreach (var item in optionValues) _context.Entry(item).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveStylistServicePriceVariantOptionValuesAsync(List<(long StylistServicePriceVariantId, long ServiceOptionValueId)> ids)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var rows = new List<StylistServicePriceVariantOptionValue>();
                foreach (var id in ids)
                {
                    var row = await GetStylistServicePriceVariantOptionValueByIdAsync(id.StylistServicePriceVariantId, id.ServiceOptionValueId);
                    if (row.Result != null) rows.Add(row.Result);
                }

                if (!rows.Any())
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching stylist service price variant option values found to remove.";
                    return result;
                }

                result = await RemoveStylistServicePriceVariantOptionValuesAsync(rows);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistStylistServicePriceVariantOptionValueAsync(long stylistServicePriceVariantId, long serviceOptionValueId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.StylistServicePriceVariantOptionValues.AsNoTracking()
                    .AnyAsync(x => x.StylistServicePriceVariantID == stylistServicePriceVariantId && x.ServiceOptionValueID == serviceOptionValueId);
                result.ID = stylistServicePriceVariantId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<StylistServicePriceVariantOptionValue>> GetAllStylistServicePriceVariantOptionValuesAsync(long stylistServicePriceVariantId = 0, long serviceOptionValueId = 0, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            ListResultObject<StylistServicePriceVariantOptionValue> results = new ListResultObject<StylistServicePriceVariantOptionValue>();
            try
            {
                var query = _context.StylistServicePriceVariantOptionValues
                    .Include(x => x.StylistServicePriceVariant)
                    .Include(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .AsNoTracking();

                if (stylistServicePriceVariantId > 0) query = query.Where(x => x.StylistServicePriceVariantID == stylistServicePriceVariantId);
                if (serviceOptionValueId > 0) query = query.Where(x => x.ServiceOptionValueID == serviceOptionValueId);

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.StylistServicePriceVariant.Price.ToString().Contains(searchText) ||
                        x.ServiceOptionValue.ValueName.Contains(searchText) ||
                        x.ServiceOptionValue.ServiceOption.OptionName.Contains(searchText));
                }

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.StylistServicePriceVariantID).SortBy(sortQuery).ToPaging(pageIndex, pageSize).ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<StylistServicePriceVariantOptionValue>> GetStylistServicePriceVariantOptionValueByIdAsync(long stylistServicePriceVariantId, long serviceOptionValueId)
        {
            RowResultObject<StylistServicePriceVariantOptionValue> result = new RowResultObject<StylistServicePriceVariantOptionValue>();
            try
            {
                result.Result = await _context.StylistServicePriceVariantOptionValues
                    .Include(x => x.StylistServicePriceVariant)
                    .Include(x => x.ServiceOptionValue).ThenInclude(x => x.ServiceOption)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.StylistServicePriceVariantID == stylistServicePriceVariantId && x.ServiceOptionValueID == serviceOptionValueId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        private static List<StylistServicePriceVariantOptionValue> NormalizeRows(IEnumerable<StylistServicePriceVariantOptionValue>? optionValues)
        {
            return optionValues?
                .Where(x => x.StylistServicePriceVariantID > 0 && x.ServiceOptionValueID > 0)
                .GroupBy(x => new { x.StylistServicePriceVariantID, x.ServiceOptionValueID })
                .Select(x => x.First())
                .ToList() ?? new List<StylistServicePriceVariantOptionValue>();
        }

        private async Task<string> ValidateRowsAsync(List<StylistServicePriceVariantOptionValue> optionValues)
        {
            var variantIds = optionValues.Select(x => x.StylistServicePriceVariantID).Distinct().ToList();
            var variants = await _context.StylistServicePriceVariants
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

            foreach (var group in optionValues.GroupBy(x => x.StylistServicePriceVariantID))
            {
                var variant = variants.First(x => x.ID == group.Key);
                var rootServiceManagementId = await GetRootServiceManagementIdAsync(variant.ServiceManagementID);
                if (rootServiceManagementId <= 0)
                {
                    return "خدمت انتخاب شده معتبر نیست";
                }

                var selectedOptionRows = optionRows
                    .Where(x => group.Select(g => g.ServiceOptionValueID).Contains(x.ID))
                    .ToList();

                if (selectedOptionRows.Any(x => x.ServiceManagementID != rootServiceManagementId))
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
            List<StylistServicePriceVariantOptionValue>? addedRows = null,
            List<StylistServicePriceVariantOptionValue>? removedRows = null,
            bool replaceExisting = false)
        {
            var variants = await _context.StylistServicePriceVariants
                .Where(x => variantIds.Contains(x.ID))
                .ToListAsync();

            var existingRows = replaceExisting
                ? new List<StylistServicePriceVariantOptionValue>()
                : await _context.StylistServicePriceVariantOptionValues
                    .AsNoTracking()
                    .Where(x => variantIds.Contains(x.StylistServicePriceVariantID))
                    .ToListAsync();

            addedRows ??= new List<StylistServicePriceVariantOptionValue>();
            removedRows ??= new List<StylistServicePriceVariantOptionValue>();

            foreach (var variant in variants)
            {
                var optionValueIds = existingRows
                    .Where(x => x.StylistServicePriceVariantID == variant.ID)
                    .Select(x => x.ServiceOptionValueID)
                    .Concat(addedRows
                        .Where(x => x.StylistServicePriceVariantID == variant.ID)
                        .Select(x => x.ServiceOptionValueID))
                    .Except(removedRows
                        .Where(x => x.StylistServicePriceVariantID == variant.ID)
                        .Select(x => x.ServiceOptionValueID))
                    .Distinct()
                    .ToList();

                variant.OptionValueCombinationKey = StylistServicePriceVariant.BuildOptionValueCombinationKey(optionValueIds);
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
