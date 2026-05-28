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
                await _context.StylistServicePriceVariantOptionValues.AddRangeAsync(optionValues);
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

        public async Task<BitResultObject> EditStylistServicePriceVariantOptionValuesAsync(List<StylistServicePriceVariantOptionValue> optionValues)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.StylistServicePriceVariantOptionValues.UpdateRange(optionValues);
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
    }
}
