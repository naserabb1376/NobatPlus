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

        public async Task<ListResultObject<StylistServicePriceVariant>> GetAllStylistServicePriceVariantsAsync(long stylistId = 0, long serviceManagementId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
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
    }
}
