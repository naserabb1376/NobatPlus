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

        public async Task<ListResultObject<StylistServiceWithDiscountDto>> GetAllStylistServicesAsync(
    long customerId = 0,
    long bookingId = 0,
    long discountId = 0,
    int pageIndex = 1,
    int pageSize = 20,
    string searchText = "",
    string sortQuery = ""
)
        {
            var results = new ListResultObject<StylistServiceWithDiscountDto>();
            try
            {
                var now = DateTime.Now; // یا UtcNow طبق سیاست پروژه‌ات

                var query = _context.StylistServices
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.ServiceManagement).ThenInclude(x => x.BookingServices)
                    .AsNoTracking()
                    .AsQueryable();

                if (bookingId > 0)
                {
                    query = query.Where(ss =>
                        ss.ServiceManagement.BookingServices.Any(bs => bs.BookingID == bookingId)
                    );
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        (x.ServiceManagement.ServiceName != null && x.ServiceManagement.ServiceName.Contains(searchText)) ||
                        (x.ServiceManagement.Description != null && x.ServiceManagement.Description.Contains(searchText)) ||
                        (x.Stylist.Specialty != null && x.Stylist.Specialty.Contains(searchText)) ||
                        (x.Stylist.StylistName != null && x.Stylist.StylistName.Contains(searchText)) ||
                        (x.Stylist.Specialty != null && x.Stylist.Specialty.Contains(searchText)) ||
                        (x.Stylist.Person.FirstName != null && x.Stylist.Person.FirstName.Contains(searchText)) ||
                        (x.Stylist.Person.LastName != null && x.Stylist.Person.LastName.Contains(searchText))
                    );
                }

                // ✅ Projection to DTO + Discount calc
                var dtoQuery = query.Select(ss => new StylistServiceWithDiscountDto
                {
                    StylistID = ss.StylistID,
                    ServiceManagementID = ss.ServiceManagementID,

                    ServiceTitle = ss.ServiceManagement.ServiceName,
                    ServiceDescription = ss.ServiceManagement.Description ?? "",

                    SalonName = ss.Stylist.StylistName,
                    StylistName = $"{ss.Stylist.Person.FirstName} {ss.Stylist.Person.LastName}",

                    ServicePrice = ss.ServicePrice,
                    ServiceDuration = ss.ServiceDuration,
                    DepositPercent = ss.DepositPercent,
                    
                    

                    DiscountPercent =
                        GetApplicableDiscountPercentsQuery(
                            ss.StylistID,
                            ss.ServiceManagementID,
                            customerId,
                            discountId,
                            now
                        )
                        .DefaultIfEmpty(0)
                        .Max(),

                    PriceAfterDiscount =
                        ss.ServicePrice *
                        (1m - (
                            GetApplicableDiscountPercentsQuery(
                                ss.StylistID,
                                ss.ServiceManagementID,
                                customerId,
                                discountId,
                                now
                            )
                            .DefaultIfEmpty(0)
                            .Max() / 100m
                        ))
                });

                // شمارش و صفحه‌بندی
                results.TotalCount = await dtoQuery.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                results.Results = await dtoQuery
                    // اگر SortBy فقط روی Entity کار می‌کنه باید SortBy را بعداً برای DTO سازگار کنی
                    .OrderByDescending(x => x.ServiceManagementID) // یا CreateDate اگر داخل DTO آوردی
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

        public async Task<RowResultObject<StylistServiceWithDiscountDto>> GetStylistServiceByIdAsync(
      long stylistId,
      long serviceManagementId,
      long customerId = 0,
      long discountId = 0
  )
        {
            var result = new RowResultObject<StylistServiceWithDiscountDto>();
            try
            {
                var now = DateTime.Now;

                result.Result = await _context.StylistServices.Include(x=> x.ServiceManagement).Include(x=> x.Stylist)
                    .AsNoTracking()
                    .Where(x => x.StylistID == stylistId && x.ServiceManagementID == serviceManagementId)
                    .Select(ss => new StylistServiceWithDiscountDto
                    {
                        StylistID = ss.StylistID,
                        ServiceManagementID = ss.ServiceManagementID,

                        ServiceTitle = ss.ServiceManagement.ServiceName,
                        ServiceDescription = ss.ServiceManagement.Description,

                        SalonName = ss.Stylist.StylistName,
                        StylistName = $"{ss.Stylist.Person.FirstName} {ss.Stylist.Person.LastName}",

                        ServicePrice = ss.ServicePrice,
                        ServiceDuration = ss.ServiceDuration,
                        DepositPercent = ss.DepositPercent,

                        DiscountPercent =
                            GetApplicableDiscountPercentsQuery(
                                ss.StylistID,
                                ss.ServiceManagementID,
                                customerId,
                                discountId,
                                now
                            )
                            .DefaultIfEmpty(0)
                            .Max(),
                        
                        PriceAfterDiscount =
                            ss.ServicePrice *
                            (1m - (
                                GetApplicableDiscountPercentsQuery(
                                    ss.StylistID,
                                    ss.ServiceManagementID,
                                    customerId,
                                    discountId,
                                    now
                                )
                                .DefaultIfEmpty(0)
                                .Max() / 100m
                            ))
                    })
                    .SingleOrDefaultAsync();
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

        private IQueryable<int> GetApplicableDiscountPercentsQuery(
    long stylistId,
    long serviceManagementId,
    long customerId,
    long discountId,
    DateTime now
)
        {
            // حالت 1: تخفیف‌های سرویس
            var serviceDiscounts =
                from sd in _context.ServiceDiscounts
                join d in _context.Discounts on sd.DiscountId equals d.ID
                where sd.ServiceManagementId == serviceManagementId
                      && (sd.StylistId == null || sd.StylistId == stylistId)
                      && d.StartDate <= now && d.EndDate >= now
                      && (
                            (discountId <= 0 && d.CodeRequired == false) ||
                            (discountId > 0 && d.ID == discountId)
                         )
                select d.DiscountAmount;

            // حالت 2: تخفیف‌های مشتری
            var customerDiscounts =
                from cd in _context.CustomerDiscounts
                join d in _context.Discounts on cd.DiscountId equals d.ID
                where ( customerId > 0 && cd.CustomerId == customerId)
                      && cd.StylistId == stylistId
                      && d.StartDate <= now && d.EndDate >= now
                      && (
                            (discountId <= 0 && d.CodeRequired == false) ||
                            (discountId > 0 && d.ID == discountId)
                         )
                select d.DiscountAmount;

            // حالت 3: تخفیف‌های عمومی (assignment)
            var assignmentDiscounts =
                from da in _context.DiscountAssignments
                join d in _context.Discounts on da.DiscountId equals d.ID
                where (da.StylistId == stylistId
                       // اگر می‌خوای AdminId هم "عمومی" حساب شود:
                       || (da.StylistId == null && da.AdminId != null))
                      && d.StartDate <= now && d.EndDate >= now
                      && (
                            (discountId <= 0 && d.CodeRequired == false) ||
                            (discountId > 0 && d.ID == discountId)
                         )
                select d.DiscountAmount;

            return serviceDiscounts
                .Concat(customerDiscounts)
                .Concat(assignmentDiscounts);
        }

    }
}
