using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class ServiceOptionRep : IServiceOptionRep
    {
        private readonly NobatPlusContext _context;

        public ServiceOptionRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddServiceOptionAsync(ServiceOption serviceOption)
        {
            var result = new BitResultObject();
            try
            {
                await _context.ServiceOptions.AddAsync(serviceOption);
                await _context.SaveChangesAsync();
                result.ID = serviceOption.ID;
                _context.Entry(serviceOption).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditServiceOptionAsync(ServiceOption serviceOption)
        {
            var result = new BitResultObject();
            try
            {
                _context.ServiceOptions.Update(serviceOption);
                await _context.SaveChangesAsync();
                result.ID = serviceOption.ID;
                _context.Entry(serviceOption).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistServiceOptionAsync(long serviceOptionId)
        {
            var result = new BitResultObject();
            try
            {
                result.Status = await _context.ServiceOptions.AsNoTracking().AnyAsync(x => x.ID == serviceOptionId);
                result.ID = serviceOptionId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<ServiceOption>> GetAllServiceOptionsAsync(long serviceManagementId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            var results = new ListResultObject<ServiceOption>();
            try
            {
                var query = _context.ServiceOptions
                    .AsNoTracking()
                    .Include(x => x.ServiceManagement)
                    .Include(x => x.Values)
                    .AsQueryable();

                if (serviceManagementId > 0)
                    query = query.Where(x => x.ServiceManagementID == serviceManagementId);

                if (isActive >= 0)
                    query = query.Where(x => x.IsActive == (isActive == 1));

                if (!string.IsNullOrWhiteSpace(searchText))
                    query = query.Where(x =>
                        x.OptionName.Contains(searchText) ||
                        x.OptionKey.Contains(searchText) ||
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

        public async Task<RowResultObject<ServiceOption>> GetServiceOptionByIdAsync(long serviceOptionId)
        {
            var result = new RowResultObject<ServiceOption>();
            try
            {
                result.Result = await _context.ServiceOptions
                    .AsNoTracking()
                    .Include(x => x.ServiceManagement)
                    .Include(x => x.Values)
                    .SingleOrDefaultAsync(x => x.ID == serviceOptionId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveServiceOptionAsync(long serviceOptionId)
        {
            var result = new BitResultObject();
            try
            {
                var serviceOption = await _context.ServiceOptions.SingleOrDefaultAsync(x => x.ID == serviceOptionId);
                if (serviceOption == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "Service option not found";
                    return result;
                }

                _context.ServiceOptions.Remove(serviceOption);
                await _context.SaveChangesAsync();
                result.ID = serviceOptionId;
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
