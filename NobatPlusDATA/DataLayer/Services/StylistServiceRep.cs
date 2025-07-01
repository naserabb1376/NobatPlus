using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.DataLayer.Services
{
    public class StylistServiceRep : IStylistServiceRep
    {

        private NobatPlusContext _context;
        public StylistServiceRep()
        {
            _context = DbTools.GetDbContext();
        }

        public async Task<BitResultObject> AddStylistServicesAsync(List<StylistService> stylistServices)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                await _context.StylistServices.AddRangeAsync(stylistServices);
                await _context.SaveChangesAsync();
                result.ID = stylistServices.FirstOrDefault().ServiceManagementID;
                foreach (var stylistService in stylistServices)
                {
                    _context.Entry(stylistService).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditStylistServicesAsync(List<StylistService> stylistServices)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.StylistServices.UpdateRange(stylistServices);
                await _context.SaveChangesAsync();
                result.ID = stylistServices.FirstOrDefault().ServiceManagementID;
                foreach (var stylistService in stylistServices)
                {
                    _context.Entry(stylistService).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveStylistServicesAsync(List<StylistService> stylistServices)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                _context.StylistServices.RemoveRange(stylistServices);
                await _context.SaveChangesAsync();
                result.ID = stylistServices.FirstOrDefault().ServiceManagementID;
                foreach (var stylistService in stylistServices)
                {
                    _context.Entry(stylistService).State = EntityState.Detached;
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveStylistServicesAsync(List<(long StylistId, long ServiceManagementId)> stylistServiceIds)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                var stylistServicesToRemove = new List<StylistService>();

                foreach (var (stylistId, serviceManagementId) in stylistServiceIds)
                {
                    var stylistService = await GetStylistServiceByIdAsync(stylistId, serviceManagementId);
                    if (stylistService.Result != null)
                    {
                        stylistServicesToRemove.Add(stylistService.Result);
                    }
                }

                if (stylistServicesToRemove.Any())
                {
                    result = await RemoveStylistServicesAsync(stylistServicesToRemove);
                }
                else
                {
                    result.Status = false;
                    result.ErrorMessage = "No matching stylist services found to remove.";
                }
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }



        public async Task<BitResultObject> ExistStylistServiceAsync(long StylistId, long ServiceManagementId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.StylistServices.AnyAsync(x => x.StylistID == StylistId && x.ServiceManagementID == ServiceManagementId);
                result.ID = ServiceManagementId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
         
        }

        public async Task<ListResultObject<StylistService>> GetAllStylistServicesAsync(int pageIndex = 1, int pageSize = 20, string searchText = "",string sortQuery ="")
        {
            ListResultObject<StylistService> results = new ListResultObject<StylistService>();
            try
            {
                var query = _context.StylistServices
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement)
                .AsNoTracking()
                .Where(x =>
                    (!string.IsNullOrEmpty(x.ServiceManagement.ServiceName) && x.ServiceManagement.ServiceName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.ServiceManagement.Description) && x.ServiceManagement.Description.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.ServiceDuration.ToString()) && x.ServiceDuration.ToString().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Specialty) && x.Stylist.Specialty.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Person.FirstName) && x.Stylist.Person.FirstName.Contains(searchText)) ||
                    (!string.IsNullOrEmpty(x.Stylist.Person.LastName) && x.Stylist.Person.LastName.Contains(searchText))
                );

                results.TotalCount = query.Count();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query.OrderByDescending(x => x.ServiceManagement.CreateDate)
                .SortBy(sortQuery).ToPaging(pageIndex, pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
           
        }

        public async Task<RowResultObject<StylistService>> GetStylistServiceByIdAsync(long StylistId, long ServiceManagementId)
        {
            RowResultObject<StylistService> result = new RowResultObject<StylistService>();
            try
            {
                result.Result = await _context.StylistServices
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement)
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.StylistID == StylistId && x.ServiceManagementID == ServiceManagementId);
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
