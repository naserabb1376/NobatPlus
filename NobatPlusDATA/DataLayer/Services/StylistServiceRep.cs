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
        public StylistServiceRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddStylistServicesAsync(List<StylistService> stylistServices)
        {
            BitResultObject result = new BitResultObject();

            try
            {
                // گروه‌بندی بر اساس Stylist
                var groupedByStylist = stylistServices
                    .GroupBy(x => x.StylistID)
                    .ToList();

                foreach (var stylistGroup in groupedByStylist)
                {
                    var stylistId = stylistGroup.Key;

                    // سرویس‌های موجود این stylist
                    var existingServices = await _context.StylistServices
                        .AsNoTracking()
                        .Where(x => x.StylistID == stylistId)
                        .ToListAsync();

                    var existingServiceIds = existingServices
                        .Select(x => x.ServiceManagementID)
                        .ToHashSet();

                    var servicesToInsert = new List<StylistService>();

                    foreach (var inputService in stylistGroup)
                    {
                        var hierarchy = await GetServiceHierarchyAsync(inputService.ServiceManagementID);

                        foreach (var service in hierarchy)
                        {
                            if (existingServiceIds.Contains(service.ID))
                                continue;

                            // اگر parent است → قیمت و مدت صفر
                            bool isParent = service.ID != inputService.ServiceManagementID;

                            servicesToInsert.Add(new StylistService
                            {
                                StylistID = stylistId,
                                ServiceManagementID = service.ID,
                                ServicePrice = isParent ? 0 : inputService.ServicePrice,
                                DepositPercent = isParent ? 0 : inputService.DepositPercent,
                                ServiceDuration = isParent ? TimeSpan.Zero : inputService.ServiceDuration
                            });

                            existingServiceIds.Add(service.ID);
                        }
                    }

                    if (servicesToInsert.Any())
                    {
                        await _context.StylistServices.AddRangeAsync(servicesToInsert);
                        await _context.SaveChangesAsync();
                    }
                }

                result.Status = true;
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
                // گروه‌بندی بر اساس Stylist
                var groupedByStylist = stylistServices
                    .GroupBy(x => x.StylistID)
                    .ToList();

                foreach (var stylistGroup in groupedByStylist)
                {
                    var stylistId = stylistGroup.Key;

                    // حذف همه سرویس‌های قبلی این stylist
                    var oldItems = await _context.StylistServices
                        .Where(x => x.StylistID == stylistId)
                        .ToListAsync();

                    if (oldItems.Any())
                    {
                        _context.StylistServices.RemoveRange(oldItems);
                        await _context.SaveChangesAsync();
                    }

                    // افزودن مجدد فقط سرویس‌های مربوط به همین stylist
                    await AddStylistServicesAsync(stylistGroup.ToList());
                }

                result.Status = true;
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
                // گروه‌بندی بر اساس Stylist
                var groupedByStylist = stylistServices
                    .GroupBy(x => x.StylistID)
                    .ToList();

                foreach (var stylistGroup in groupedByStylist)
                {
                    var stylistId = stylistGroup.Key;
                    var serviceIdsToRemove = new HashSet<long>();

                    foreach (var item in stylistGroup)
                    {
                        // خود service
                        serviceIdsToRemove.Add(item.ServiceManagementID);

                        // همه childها
                        var descendants = await GetServiceDescendantsAsync(item.ServiceManagementID);
                        foreach (var child in descendants)
                            serviceIdsToRemove.Add(child.ID);
                    }

                    var itemsToRemove = await _context.StylistServices
                        .Where(x =>
                            x.StylistID == stylistId &&
                            serviceIdsToRemove.Contains(x.ServiceManagementID))
                        .ToListAsync();

                    if (itemsToRemove.Any())
                    {
                        _context.StylistServices.RemoveRange(itemsToRemove);
                        await _context.SaveChangesAsync();
                    }
                }

                result.Status = true;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        public async Task<BitResultObject> RemoveStylistServicesAsync(
      List<(long StylistId, long ServiceManagementId)> stylistServiceIds)
        {
            BitResultObject result = new BitResultObject();

            try
            {
                var stylistServices = stylistServiceIds
                    .Select(x => new StylistService
                    {
                        StylistID = x.StylistId,
                        ServiceManagementID = x.ServiceManagementId
                    })
                    .ToList();

                result = await RemoveStylistServicesAsync(stylistServices);
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

        private async Task<List<ServiceManagement>> GetServiceHierarchyAsync(long serviceId)
        {
            var result = new List<ServiceManagement>();

            var current = await _context.ServiceManagements
                .FirstOrDefaultAsync(x => x.ID == serviceId);

            while (current != null && current.ID != 0)
            {
                result.Add(current);

                if (current.ServiceParentID == 0)
                    break;

                current = await _context.ServiceManagements
                    .FirstOrDefaultAsync(x => x.ID == current.ServiceParentID);
            }

            return result;
        }

        private async Task<List<ServiceManagement>> GetServiceDescendantsAsync(long serviceId)
        {
            var result = new List<ServiceManagement>();

            async Task LoadChildren(long parentId)
            {
                var children = await _context.ServiceManagements
                    .Where(x => x.ServiceParentID == parentId)
                    .ToListAsync();

                foreach (var child in children)
                {
                    result.Add(child);
                    await LoadChildren(child.ID);
                }
            }

            await LoadChildren(serviceId);
            return result;
        }

    }
}
