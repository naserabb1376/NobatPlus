using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class StylistServicePriceVariantRep : IStylistServicePriceVariantRep
    {
        private readonly NobatPlusContext _context;

        public StylistServicePriceVariantRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddStylistServicePriceVariantAsync(StylistServicePriceVariant stylistServicePriceVariant)
        {
            var result = new BitResultObject();
            try
            {
                var validationError = await ValidateVariantAsync(stylistServicePriceVariant);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                await _context.StylistServicePriceVariants.AddAsync(stylistServicePriceVariant);
                await _context.SaveChangesAsync();
                result.ID = stylistServicePriceVariant.ID;
                _context.Entry(stylistServicePriceVariant).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditStylistServicePriceVariantAsync(StylistServicePriceVariant stylistServicePriceVariant)
        {
            var result = new BitResultObject();
            try
            {
                var validationError = await ValidateVariantAsync(stylistServicePriceVariant, stylistServicePriceVariant.ID);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                var oldOptions = await _context.StylistServicePriceVariantOptionValues
                    .Where(x => x.StylistServicePriceVariantID == stylistServicePriceVariant.ID)
                    .ToListAsync();

                _context.StylistServicePriceVariantOptionValues.RemoveRange(oldOptions);
                _context.StylistServicePriceVariants.Update(stylistServicePriceVariant);
                await _context.SaveChangesAsync();
                result.ID = stylistServicePriceVariant.ID;
                _context.Entry(stylistServicePriceVariant).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistStylistServicePriceVariantAsync(long stylistServicePriceVariantId)
        {
            var result = new BitResultObject();
            try
            {
                result.Status = await _context.StylistServicePriceVariants.AsNoTracking().AnyAsync(x => x.ID == stylistServicePriceVariantId);
                result.ID = stylistServicePriceVariantId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<StylistServicePriceVariant>> GetAllStylistServicePriceVariantsAsync(long stylistId = 0, long serviceManagementId = 0, int isActive = -1, bool onlyLeafServices = false, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            var results = new ListResultObject<StylistServicePriceVariant>();
            try
            {
                var query = _context.StylistServicePriceVariants
                    .AsNoTracking()
                    .Include(x => x.OptionValues)
                    .ThenInclude(x => x.ServiceOptionValue)
                    .ThenInclude(x => x.ServiceOption)
                    .AsQueryable();

                if (stylistId > 0)
                    query = query.Where(x => x.StylistID == stylistId);

                if (serviceManagementId > 0)
                    query = query.Where(x => x.ServiceManagementID == serviceManagementId);

                if (onlyLeafServices)
                    query = query.Where(x => !_context.ServiceManagements.Any(child => child.ServiceParentID == x.ServiceManagementID));

                if (isActive >= 0)
                    query = query.Where(x => x.IsActive == (isActive == 1));

                if (!string.IsNullOrWhiteSpace(searchText))
                    query = query.Where(x => x.Description != null && x.Description.Contains(searchText));

                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query
                    .OrderByDescending(x => x.ID)
                    .SortBy(sortQuery)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<StylistServicePriceVariant>> GetStylistServicePriceVariantByIdAsync(long stylistServicePriceVariantId)
        {
            var result = new RowResultObject<StylistServicePriceVariant>();
            try
            {
                result.Result = await _context.StylistServicePriceVariants
                    .AsNoTracking()
                    .Include(x => x.OptionValues)
                    .ThenInclude(x => x.ServiceOptionValue)
                    .ThenInclude(x => x.ServiceOption)
                    .SingleOrDefaultAsync(x => x.ID == stylistServicePriceVariantId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveStylistServicePriceVariantAsync(long stylistServicePriceVariantId)
        {
            var result = new BitResultObject();
            try
            {
                var variant = await _context.StylistServicePriceVariants.SingleOrDefaultAsync(x => x.ID == stylistServicePriceVariantId);
                if (variant == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "Stylist service price variant not found";
                    return result;
                }

                _context.StylistServicePriceVariants.Remove(variant);
                await _context.SaveChangesAsync();
                result.ID = stylistServicePriceVariantId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        private async Task<string> ValidateVariantAsync(StylistServicePriceVariant variant, long excludedVariantId = 0)
        {
            var optionValueIds = variant.OptionValues?
                .Select(x => x.ServiceOptionValueID)
                .Where(x => x > 0)
                .Distinct()
                .OrderBy(x => x)
                .ToList() ?? new List<long>();

            variant.OptionValueCombinationKey = StylistServicePriceVariant.BuildOptionValueCombinationKey(optionValueIds);

            if (!optionValueIds.Any())
                return "حداقل یک گزینه برای قیمت متغیر خدمت باید انتخاب شود";

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
                return "یک یا چند مقدار گزینه انتخاب شده معتبر نیست";

            var rootServiceManagementId = await GetRootServiceManagementIdAsync(variant.ServiceManagementID);
            if (rootServiceManagementId <= 0)
                return "خدمت انتخاب شده معتبر نیست";

            if (optionRows.Any(x => x.ServiceManagementID != rootServiceManagementId))
                return "گزینه‌های انتخاب شده باید متعلق به همان خدمت باشند";

            if (optionRows.GroupBy(x => x.ServiceOptionID).Any(x => x.Count() > 1))
                return "برای هر ویژگی فقط یک مقدار قابل انتخاب است";

            var duplicateExists = await _context.StylistServicePriceVariants
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ID != excludedVariantId &&
                    x.StylistID == variant.StylistID &&
                    x.ServiceManagementID == variant.ServiceManagementID &&
                    x.OptionValueCombinationKey == variant.OptionValueCombinationKey);

            return duplicateExists
                ? "این خدمات قبلا ثبت شده است و تکراری است"
                : "";
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
