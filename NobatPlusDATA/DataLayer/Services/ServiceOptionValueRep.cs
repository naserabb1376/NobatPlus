using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class ServiceOptionValueRep : IServiceOptionValueRep
    {
        private readonly NobatPlusContext _context;

        public ServiceOptionValueRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddServiceOptionValueAsync(ServiceOptionValue serviceOptionValue)
        {
            var result = new BitResultObject();
            try
            {
                await _context.ServiceOptionValues.AddAsync(serviceOptionValue);
                await _context.SaveChangesAsync();
                result.ID = serviceOptionValue.ID;
                _context.Entry(serviceOptionValue).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditServiceOptionValueAsync(ServiceOptionValue serviceOptionValue)
        {
            var result = new BitResultObject();
            try
            {
                _context.ServiceOptionValues.Update(serviceOptionValue);
                await _context.SaveChangesAsync();
                result.ID = serviceOptionValue.ID;
                _context.Entry(serviceOptionValue).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistServiceOptionValueAsync(long serviceOptionValueId)
        {
            var result = new BitResultObject();
            try
            {
                result.Status = await _context.ServiceOptionValues.AsNoTracking().AnyAsync(x => x.ID == serviceOptionValueId);
                result.ID = serviceOptionValueId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<ServiceOptionValue>> GetAllServiceOptionValuesAsync(long serviceOptionId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            var results = new ListResultObject<ServiceOptionValue>();
            try
            {
                var query = _context.ServiceOptionValues
                    .AsNoTracking()
                    .Include(x => x.ServiceOption)
                    .AsQueryable();

                if (serviceOptionId > 0)
                    query = query.Where(x => x.ServiceOptionID == serviceOptionId);

                if (isActive >= 0)
                    query = query.Where(x => x.IsActive == (isActive == 1));

                if (!string.IsNullOrWhiteSpace(searchText))
                    query = query.Where(x =>
                        x.ValueName.Contains(searchText) ||
                        (x.Description != null && x.Description.Contains(searchText)));

                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query
                    .OrderBy(x => x.SortOrder)
                    .ThenByDescending(x => x.ID)
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

        public async Task<RowResultObject<ServiceOptionValue>> GetServiceOptionValueByIdAsync(long serviceOptionValueId)
        {
            var result = new RowResultObject<ServiceOptionValue>();
            try
            {
                result.Result = await _context.ServiceOptionValues
                    .AsNoTracking()
                    .Include(x => x.ServiceOption)
                    .SingleOrDefaultAsync(x => x.ID == serviceOptionValueId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveServiceOptionValueAsync(long serviceOptionValueId)
        {
            var result = new BitResultObject();
            try
            {
                var serviceOptionValue = await _context.ServiceOptionValues.SingleOrDefaultAsync(x => x.ID == serviceOptionValueId);
                if (serviceOptionValue == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "Service option value not found";
                    return result;
                }

                _context.ServiceOptionValues.Remove(serviceOptionValue);
                await _context.SaveChangesAsync();
                result.ID = serviceOptionValueId;
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
