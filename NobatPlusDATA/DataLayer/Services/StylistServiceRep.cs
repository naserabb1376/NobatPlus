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
                    var priceVariantsToInsert = new List<StylistServicePriceVariant>();

                    foreach (var inputService in stylistGroup)
                    {
                        var hierarchy = await GetServiceHierarchyAsync(inputService.ServiceManagementID);

                        foreach (var service in hierarchy)
                        {
                            if (existingServiceIds.Contains(service.ID))
                            {
                                if (service.ID == inputService.ServiceManagementID)
                                {
                                    if (!inputService.HasDynamicPricing)
                                    {
                                        result.Status = false;
                                        result.ErrorMessage = $"این اطلاعات (انجام دهنده خدمت: {inputService.StylistID}, خدمت: {inputService.ServiceManagementID}) قبلا در سیسستم ثبت شده است";
                                        return result;
                                    }

                                    priceVariantsToInsert.AddRange(BuildPriceVariantsForInsert(inputService, stylistId, service.ID));
                                }

                                continue;
                            }

                            // اگر parent است → قیمت و مدت صفر
                            bool isParent = service.ID != inputService.ServiceManagementID;

                            servicesToInsert.Add(new StylistService
                            {
                                StylistID = stylistId,
                                ServiceManagementID = service.ID,
                                ServicePrice = isParent ? 0 : inputService.ServicePrice,
                                DepositPercent = isParent ? 0 : inputService.DepositPercent,
                                ServiceDuration = isParent ? TimeSpan.Zero : inputService.ServiceDuration,
                                HasDynamicPricing = !isParent && inputService.HasDynamicPricing,
                                PriceVariants = isParent
                                    ? new List<StylistServicePriceVariant>()
                                    : BuildPriceVariantsForInsert(inputService, stylistId, service.ID)
                            });

                            existingServiceIds.Add(service.ID);
                        }
                    }

                    var allPriceVariantsToInsert = servicesToInsert
                        .SelectMany(x => x.PriceVariants ?? new List<StylistServicePriceVariant>())
                        .Concat(priceVariantsToInsert)
                        .ToList();

                    if (allPriceVariantsToInsert.Any())
                    {
                        var validationError = await ValidatePriceVariantsAsync(allPriceVariantsToInsert);
                        if (!string.IsNullOrEmpty(validationError))
                        {
                            result.Status = false;
                            result.ErrorMessage = validationError;
                            return result;
                        }
                    }

                    if (servicesToInsert.Any())
                        await _context.StylistServices.AddRangeAsync(servicesToInsert);

                    if (priceVariantsToInsert.Any())
                        await _context.StylistServicePriceVariants.AddRangeAsync(priceVariantsToInsert);

                    if (servicesToInsert.Any() || priceVariantsToInsert.Any())
                        await _context.SaveChangesAsync();
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

        private static List<StylistServicePriceVariant> BuildPriceVariantsForInsert(
            StylistService source,
            long stylistId,
            long serviceManagementId)
        {
            return source.PriceVariants?.Select(variant => new StylistServicePriceVariant
            {
                CreateDate = variant.CreateDate ?? DateTime.Now.ToShamsi(),
                UpdateDate = variant.UpdateDate ?? DateTime.Now.ToShamsi(),
                Description = variant.Description,
                StylistID = stylistId,
                ServiceManagementID = serviceManagementId,
                Price = variant.Price,
                Duration = variant.Duration,
                DepositPercent = variant.DepositPercent,
                IsActive = variant.IsActive,
                OptionValueCombinationKey = StylistServicePriceVariant.BuildOptionValueCombinationKey(
                    variant.OptionValues?.Select(x => x.ServiceOptionValueID)),
                OptionValues = variant.OptionValues?.Select(optionValue => new StylistServicePriceVariantOptionValue
                {
                    ServiceOptionValueID = optionValue.ServiceOptionValueID
                }).ToList() ?? new List<StylistServicePriceVariantOptionValue>()
            }).ToList() ?? new List<StylistServicePriceVariant>();
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
                    var servicesToReinsert = stylistGroup.ToList();

                    // حذف همه سرویس‌های قبلی این stylist
                    var oldItems = await _context.StylistServices
                        .Include(x => x.PriceVariants)
                            .ThenInclude(x => x.OptionValues)
                        .Where(x => x.StylistID == stylistId)
                        .ToListAsync();

                    var oldItemsByServiceId = oldItems.ToDictionary(
                        x => x.ServiceManagementID,
                        x => x);

                    var now = DateTime.Now.ToShamsi();

                    foreach (var inputService in servicesToReinsert)
                    {
                        if (!oldItemsByServiceId.TryGetValue(inputService.ServiceManagementID, out var oldItem))
                            continue;

                        if (inputService.HasDynamicPricing)
                        {
                            inputService.ServicePrice = oldItem.ServicePrice;
                            inputService.DepositPercent = oldItem.DepositPercent;
                            inputService.ServiceDuration = oldItem.ServiceDuration;
                        }

                        var requestedVariants = inputService.PriceVariants?.ToList() ??
                            new List<StylistServicePriceVariant>();

                        if (!requestedVariants.Any())
                        {
                            requestedVariants = oldItem.PriceVariants
                                .Select(variant => CloneVariantForReinsert(variant, now))
                                .ToList();
                        }
                        else
                        {
                            requestedVariants = requestedVariants
                                .Select(variant =>
                                {
                                    var combinationKey = StylistServicePriceVariant.BuildOptionValueCombinationKey(
                                        variant.OptionValues?.Select(x => x.ServiceOptionValueID));
                                    var oldVariant = oldItem.PriceVariants.FirstOrDefault(x =>
                                        (variant.ID > 0 && x.ID == variant.ID) ||
                                        x.OptionValueCombinationKey == combinationKey);

                                    variant.CreateDate = oldVariant?.CreateDate ?? now;
                                    variant.UpdateDate = now;
                                    variant.OptionValueCombinationKey = combinationKey;
                                    return variant;
                                })
                                .ToList();

                            var requestedVariantIds = requestedVariants
                                .Where(x => x.ID > 0)
                                .Select(x => x.ID)
                                .ToHashSet();

                            var requestedCombinationKeys = requestedVariants
                                .Select(x => x.OptionValueCombinationKey)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToHashSet();

                            var untouchedOldVariants = oldItem.PriceVariants
                                .Where(oldVariant =>
                                    !requestedVariantIds.Contains(oldVariant.ID) &&
                                    !requestedCombinationKeys.Contains(oldVariant.OptionValueCombinationKey))
                                .Select(variant => CloneVariantForReinsert(variant, now))
                                .ToList();

                            requestedVariants.AddRange(untouchedOldVariants);
                        }

                        inputService.PriceVariants = requestedVariants;
                    }

                    var requestedServiceIds = servicesToReinsert
                        .Select(x => x.ServiceManagementID)
                        .ToHashSet();

                    var candidateServiceIds = oldItems
                        .Select(x => x.ServiceManagementID)
                        .ToHashSet();

                    foreach (var oldItem in oldItems.Where(x => !requestedServiceIds.Contains(x.ServiceManagementID)))
                    {
                        var descendants = await GetServiceDescendantsAsync(oldItem.ServiceManagementID);
                        var isParentOfAnotherKeptService = descendants.Any(x => candidateServiceIds.Contains(x.ID));

                        if (isParentOfAnotherKeptService)
                            continue;

                        servicesToReinsert.Add(CloneStylistServiceForReinsert(oldItem, now));
                    }

                    if (oldItems.Any())
                    {
                        _context.StylistServices.RemoveRange(oldItems);
                        await _context.SaveChangesAsync();
                    }

                    // افزودن مجدد فقط سرویس‌های مربوط به همین stylist
                    var addResult = await AddStylistServicesAsync(servicesToReinsert);
                    if (!addResult.Status)
                        return addResult;
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

        private static StylistServicePriceVariant CloneVariantForReinsert(
            StylistServicePriceVariant source,
            DateTime updateDate)
        {
            return new StylistServicePriceVariant
            {
                CreateDate = source.CreateDate ?? updateDate,
                UpdateDate = updateDate,
                Description = source.Description,
                StylistID = source.StylistID,
                ServiceManagementID = source.ServiceManagementID,
                Price = source.Price,
                Duration = source.Duration,
                DepositPercent = source.DepositPercent,
                IsActive = source.IsActive,
                OptionValueCombinationKey = source.OptionValueCombinationKey,
                OptionValues = source.OptionValues
                    .Select(x => new StylistServicePriceVariantOptionValue
                    {
                        ServiceOptionValueID = x.ServiceOptionValueID
                    })
                    .ToList()
            };
        }

        private static StylistService CloneStylistServiceForReinsert(
            StylistService source,
            DateTime updateDate)
        {
            return new StylistService
            {
                StylistID = source.StylistID,
                ServiceManagementID = source.ServiceManagementID,
                ServicePrice = source.ServicePrice,
                DepositPercent = source.DepositPercent,
                ServiceDuration = source.ServiceDuration,
                HasDynamicPricing = source.HasDynamicPricing,
                PriceVariants = source.PriceVariants?
                    .Select(variant => CloneVariantForReinsert(variant, updateDate))
                    .ToList() ?? new List<StylistServicePriceVariant>()
            };
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
                    var parentIdsToCheck = new List<long>();

                    foreach (var item in stylistGroup)
                    {
                        // خود service
                        serviceIdsToRemove.Add(item.ServiceManagementID);

                        // همه childها
                        var descendants = await GetServiceDescendantsAsync(item.ServiceManagementID);
                        foreach (var child in descendants)
                            serviceIdsToRemove.Add(child.ID);

                        // والدها فقط در صورتی حذف می‌شوند که بعد از حذف این رکورد، برای سرویس دیگری لازم نباشند
                        var hierarchy = await GetServiceHierarchyAsync(item.ServiceManagementID);
                        foreach (var parent in hierarchy.Where(x => x.ID != item.ServiceManagementID))
                        {
                            if (!parentIdsToCheck.Contains(parent.ID))
                                parentIdsToCheck.Add(parent.ID);
                        }
                    }

                    var existingServiceIds = await _context.StylistServices
                        .AsNoTracking()
                        .Where(x => x.StylistID == stylistId)
                        .Select(x => x.ServiceManagementID)
                        .ToListAsync();

                    var remainingServiceIds = existingServiceIds
                        .Where(id => !serviceIdsToRemove.Contains(id))
                        .ToHashSet();

                    foreach (var parentId in parentIdsToCheck)
                    {
                        if (!remainingServiceIds.Contains(parentId))
                            continue;

                        var descendants = await GetServiceDescendantsAsync(parentId);
                        var hasRemainingDescendant = descendants.Any(x => remainingServiceIds.Contains(x.ID));
                        if (hasRemainingDescendant)
                            continue;

                        serviceIdsToRemove.Add(parentId);
                        remainingServiceIds.Remove(parentId);
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
      List<(long StylistId, long ServiceManagementId, long? stylistServicePriceVariantId)> stylistServiceIds)
        {
            BitResultObject result = new BitResultObject();

            try
            {
                var fixedStylistServices = new List<StylistService>();

                foreach (var item in stylistServiceIds)
                {
                    if (item.stylistServicePriceVariantId.HasValue && item.stylistServicePriceVariantId.Value > 0)
                    {
                        var dynamicRemoveResult = await RemoveDynamicStylistServiceAsync(
                            item.StylistId,
                            item.ServiceManagementId,
                            item.stylistServicePriceVariantId.Value);

                        if (!dynamicRemoveResult.Status)
                            return dynamicRemoveResult;

                        continue;
                    }

                    fixedStylistServices.Add(new StylistService
                    {
                        StylistID = item.StylistId,
                        ServiceManagementID = item.ServiceManagementId
                    });
                }

                if (fixedStylistServices.Any())
                {
                    result = await RemoveStylistServicesAsync(fixedStylistServices);
                    return result;
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

        private async Task<BitResultObject> RemoveDynamicStylistServiceAsync(long stylistId, long serviceManagementId, long stylistServicePriceVariantId)
        {
            var result = new BitResultObject();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var variant = await _context.StylistServicePriceVariants
                    .SingleOrDefaultAsync(x =>
                        x.ID == stylistServicePriceVariantId &&
                        x.StylistID == stylistId &&
                        x.ServiceManagementID == serviceManagementId);

                if (variant == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "ردیف قیمت متغیر برای آرایشگر و خدمت ورودی پیدا نشد";
                    return result;
                }

                var optionValues = await _context.StylistServicePriceVariantOptionValues
                    .Where(x => x.StylistServicePriceVariantID == stylistServicePriceVariantId)
                    .ToListAsync();

                if (optionValues.Any())
                    _context.StylistServicePriceVariantOptionValues.RemoveRange(optionValues);

                _context.StylistServicePriceVariants.Remove(variant);
                await _context.SaveChangesAsync();

                var hasOtherVariants = await _context.StylistServicePriceVariants
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.StylistID == stylistId &&
                        x.ServiceManagementID == serviceManagementId);

                if (!hasOtherVariants)
                    await RemoveStylistServiceAndUnusedParentsAsync(stylistId, serviceManagementId);

                await transaction.CommitAsync();

                result.Status = true;
                result.ID = stylistServicePriceVariantId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private async Task RemoveStylistServiceAndUnusedParentsAsync(long stylistId, long serviceManagementId)
        {
            var existingServiceIds = await _context.StylistServices
                .AsNoTracking()
                .Where(x => x.StylistID == stylistId)
                .Select(x => x.ServiceManagementID)
                .ToListAsync();

            var serviceIdsToRemove = new HashSet<long>();
            if (existingServiceIds.Contains(serviceManagementId))
                serviceIdsToRemove.Add(serviceManagementId);

            var remainingServiceIds = existingServiceIds
                .Where(id => !serviceIdsToRemove.Contains(id))
                .ToHashSet();

            var hierarchy = await GetServiceHierarchyAsync(serviceManagementId);
            foreach (var parent in hierarchy.Where(x => x.ID != serviceManagementId))
            {
                if (!remainingServiceIds.Contains(parent.ID))
                    continue;

                var descendants = await GetServiceDescendantsAsync(parent.ID);
                var hasRemainingDescendant = descendants.Any(x => remainingServiceIds.Contains(x.ID));
                if (hasRemainingDescendant)
                    continue;

                serviceIdsToRemove.Add(parent.ID);
                remainingServiceIds.Remove(parent.ID);
            }

            if (!serviceIdsToRemove.Any())
                return;

            var stylistServices = await _context.StylistServices
                .Where(x =>
                    x.StylistID == stylistId &&
                    serviceIdsToRemove.Contains(x.ServiceManagementID))
                .ToListAsync();

            if (stylistServices.Any())
            {
                _context.StylistServices.RemoveRange(stylistServices);
                await _context.SaveChangesAsync();
            }
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
     long stylistId = 0,
     long serviceId = 0,
     long customerId = 0,
     long bookingId = 0,
     long discountId = 0,
     List<long>? optionValueIds = null,
     bool onlyLeafServices = false,
     int pageIndex = 1,
     int pageSize = 20,
     string searchText = "",
     string sortQuery = ""
 )
        {
            var results = new ListResultObject<StylistServiceWithDiscountDto>();

            try
            {
                var now = DateTime.Now.ToShamsi();

                var query = _context.StylistServices
                    .AsNoTracking()
                    .AsQueryable();

                if (stylistId > 0)
                {
                    query = query.Where(ss => ss.StylistID == stylistId);
                }

                if (serviceId > 0)
                {
                    query = query.Where(ss => ss.ServiceManagementID == serviceId);
                }

                if (onlyLeafServices)
                {
                    query = query.Where(ss => !_context.ServiceManagements.Any(child => child.ServiceParentID == ss.ServiceManagementID));
                }

                if (bookingId > 0)
                {
                    query = query.Where(ss =>
                        ss.ServiceManagement.BookingServices.Any(bs => bs.BookingID == bookingId)
                    );
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        (x.ServiceManagement.ServiceName != null &&
                         x.ServiceManagement.ServiceName.Contains(searchText)) ||

                        (x.ServiceManagement.Description != null &&
                         x.ServiceManagement.Description.Contains(searchText)) ||

                        (x.Stylist.Specialty != null &&
                         x.Stylist.Specialty.Contains(searchText)) ||

                        (x.Stylist.StylistName != null &&
                         x.Stylist.StylistName.Contains(searchText)) ||

                        (x.Stylist.Person.FirstName != null &&
                         x.Stylist.Person.FirstName.Contains(searchText)) ||

                        (x.Stylist.Person.LastName != null &&
                         x.Stylist.Person.LastName.Contains(searchText))
                    );
                }

                optionValueIds = NormalizeOptionValueIds(optionValueIds);

                var baseQuery = query.Select(ss => new
                {
                    ss.StylistID,
                    ss.ServiceManagementID,

                    ServiceTitle = ss.ServiceManagement.ServiceName,
                    ServiceDescription = ss.ServiceManagement.Description ?? "",

                    SalonName = ss.Stylist.StylistName,

                    FirstName = ss.Stylist.Person.FirstName,
                    LastName = ss.Stylist.Person.LastName,

                    ss.ServicePrice,
                    ss.ServiceDuration,
                    ss.DepositPercent,
                    ss.HasDynamicPricing
                });

                results.TotalCount = await baseQuery.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);

                var pageItems = await baseQuery
                    .OrderByDescending(x => x.ServiceManagementID)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();

                var finalList = new List<StylistServiceWithDiscountDto>();

                foreach (var item in pageItems)
                {
                    var resolvedPricing = await ResolvePricingAsync(
                        item.StylistID,
                        item.ServiceManagementID,
                        item.ServicePrice,
                        item.ServiceDuration,
                        item.DepositPercent,
                        item.HasDynamicPricing,
                        bookingId,
                        optionValueIds);

                    var discountPercent = await GetApplicableDiscountPercentsQuery(
         item.StylistID,
         item.ServiceManagementID,
         customerId,
         discountId,
         now
     )
     .Select(x => (decimal?)x)
     .MaxAsync() ?? 0m;

                    var discountPercentDecimal = Math.Clamp(Convert.ToDecimal(discountPercent), 0m, 100m);
                    var priceVariants = item.HasDynamicPricing
                        ? await GetPriceVariantItemsAsync(item.StylistID, item.ServiceManagementID, discountPercentDecimal)
                        : new List<StylistServicePriceVariantPriceDto>();

                    var priceAfterDiscount =
                        resolvedPricing.Price * (1m - (discountPercentDecimal / 100m));

                    finalList.Add(new StylistServiceWithDiscountDto
                    {
                        StylistID = item.StylistID,
                        ServiceManagementID = item.ServiceManagementID,
                        BookingID = bookingId,

                        ServiceTitle = item.ServiceTitle,
                        ServiceDescription = item.ServiceDescription,

                        SalonName = item.SalonName,
                        StylistName = $"{item.FirstName} {item.LastName}".Trim(),

                        ServicePrice = resolvedPricing.Price,
                        ServiceDuration = resolvedPricing.Duration,
                        DepositPercent = resolvedPricing.DepositPercent,
                        HasDynamicPricing = item.HasDynamicPricing,
                        StylistServicePriceVariantID = resolvedPricing.VariantId,
                        AppliedOptionValueIDs = resolvedPricing.OptionValueIds,
                        AppliedOptionSummary = resolvedPricing.OptionSummary,

                        DiscountPercent = Convert.ToInt32(discountPercentDecimal),
                        PriceAfterDiscount = priceAfterDiscount,
                        PriceVariants = priceVariants
                    });
                }

                results.Results = finalList;
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
            long discountId = 0,
            List<long>? optionValueIds = null
        )
        {
            var result = new RowResultObject<StylistServiceWithDiscountDto>();

            try
            {
                var now = DateTime.Now.ToShamsi();

                var item = await _context.StylistServices
                    .AsNoTracking()
                    .Where(x =>
                        x.StylistID == stylistId &&
                        x.ServiceManagementID == serviceManagementId
                    )
                    .Select(ss => new
                    {
                        ss.StylistID,
                        ss.ServiceManagementID,

                        ServiceTitle = ss.ServiceManagement.ServiceName,
                        ServiceDescription = ss.ServiceManagement.Description ?? "",

                        SalonName = ss.Stylist.StylistName,

                        FirstName = ss.Stylist.Person.FirstName,
                        LastName = ss.Stylist.Person.LastName,

                        ss.ServicePrice,
                        ss.ServiceDuration,
                        ss.DepositPercent,
                        ss.HasDynamicPricing
                    })
                    .SingleOrDefaultAsync();

                if (item == null)
                {
                    result.Result = null;
                    return result;
                }

                optionValueIds = NormalizeOptionValueIds(optionValueIds);

                var resolvedPricing = await ResolvePricingAsync(
                    item.StylistID,
                    item.ServiceManagementID,
                    item.ServicePrice,
                    item.ServiceDuration,
                    item.DepositPercent,
                    item.HasDynamicPricing,
                    0,
                    optionValueIds);

                var discountPercent = await GetApplicableDiscountPercentsQuery(
                        item.StylistID,
                        item.ServiceManagementID,
                        customerId,
                        discountId,
                        now
                    )
                    .Select(x => (decimal?)x)
                    .MaxAsync() ?? 0m;

                discountPercent = Math.Clamp(discountPercent, 0m, 100m);
                var priceVariants = item.HasDynamicPricing
                    ? await GetPriceVariantItemsAsync(item.StylistID, item.ServiceManagementID, discountPercent)
                    : new List<StylistServicePriceVariantPriceDto>();

                var returnedPrice = resolvedPricing.Price;
                var priceAfterDiscount =
                    returnedPrice * (1m - (discountPercent / 100m));

                result.Result = new StylistServiceWithDiscountDto
                {
                    StylistID = item.StylistID,
                    ServiceManagementID = item.ServiceManagementID,

                    ServiceTitle = item.ServiceTitle,
                    ServiceDescription = item.ServiceDescription,

                    SalonName = item.SalonName,
                    StylistName = $"{item.FirstName} {item.LastName}".Trim(),

                    ServicePrice = returnedPrice,
                    ServiceDuration = resolvedPricing.Duration,
                    DepositPercent = resolvedPricing.DepositPercent,
                    HasDynamicPricing = item.HasDynamicPricing,
                    StylistServicePriceVariantID = resolvedPricing.VariantId,
                    AppliedOptionValueIDs = resolvedPricing.OptionValueIds,
                    AppliedOptionSummary = resolvedPricing.OptionSummary,

                    DiscountPercent = Convert.ToInt32(discountPercent),
                    PriceAfterDiscount = priceAfterDiscount,
                    PriceVariants = priceVariants
                };
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private async Task<List<StylistServicePriceVariantPriceDto>> GetPriceVariantItemsAsync(
            long stylistId,
            long serviceManagementId,
            decimal discountPercent)
        {
            discountPercent = Math.Clamp(discountPercent, 0m, 100m);

            var variants = await _context.StylistServicePriceVariants
                .AsNoTracking()
                .Include(x => x.OptionValues)
                    .ThenInclude(x => x.ServiceOptionValue)
                    .ThenInclude(x => x.ServiceOption)
                .Where(x =>
                    x.StylistID == stylistId &&
                    x.ServiceManagementID == serviceManagementId &&
                    x.IsActive)
                .OrderBy(x => x.ID)
                .ToListAsync();

            return variants.Select(variant =>
            {
                var optionValues = variant.OptionValues
                    .OrderBy(x => x.ServiceOptionValue.ServiceOption.SortOrder)
                    .ThenBy(x => x.ServiceOptionValue.SortOrder)
                    .ToList();

                var priceAfterDiscount = variant.Price * (1m - (discountPercent / 100m));

                return new StylistServicePriceVariantPriceDto
                {
                    StylistServicePriceVariantID = variant.ID,
                    ServicePrice = variant.Price,
                    ServiceDuration = variant.Duration,
                    DepositPercent = variant.DepositPercent,
                    AppliedOptionValueIDs = optionValues.Select(x => x.ServiceOptionValueID).ToList(),
                    AppliedOptionSummary = string.Join("، ", optionValues.Select(x =>
                        $"{x.ServiceOptionValue.ServiceOption.OptionName}: {x.ServiceOptionValue.ValueName}")),
                    DiscountPercent = Convert.ToInt32(discountPercent),
                    PriceAfterDiscount = priceAfterDiscount
                };
            }).ToList();
        }

        private async Task<string> ValidatePriceVariantsAsync(IEnumerable<StylistServicePriceVariant> variants)
        {
            var variantList = variants.ToList();
            var rootServiceIds = new Dictionary<long, long>();
            foreach (var variant in variantList)
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

                if (!rootServiceIds.TryGetValue(variant.ServiceManagementID, out var rootServiceManagementId))
                {
                    rootServiceManagementId = await GetRootServiceManagementIdAsync(variant.ServiceManagementID);
                    rootServiceIds[variant.ServiceManagementID] = rootServiceManagementId;
                }

                if (rootServiceManagementId <= 0)
                    return "خدمت انتخاب شده معتبر نیست";

                if (optionRows.Any(x => x.ServiceManagementID != rootServiceManagementId))
                    return "گزینه‌های انتخاب شده باید متعلق به همان خدمت باشند";

                if (optionRows.GroupBy(x => x.ServiceOptionID).Any(x => x.Count() > 1))
                    return "برای هر ویژگی فقط یک مقدار قابل انتخاب است";
            }

            var duplicateInput = variantList
                .GroupBy(x => new { x.StylistID, x.ServiceManagementID, x.OptionValueCombinationKey })
                .FirstOrDefault(x => x.Count() > 1);

                if (duplicateInput != null)
                    return "این خدمات قبلا ثبت شده است و تکراری است";

            var keysByService = variantList
                .GroupBy(x => new { x.StylistID, x.ServiceManagementID })
                .ToList();

            foreach (var group in keysByService)
            {
                var keys = group.Select(x => x.OptionValueCombinationKey).ToList();
                var exists = await _context.StylistServicePriceVariants
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.StylistID == group.Key.StylistID &&
                        x.ServiceManagementID == group.Key.ServiceManagementID &&
                        keys.Contains(x.OptionValueCombinationKey));

                if (exists)
                    return "این خدمات قبلا ثبت شده است و تکراری است";
            }

            return "";
        }

        private async Task<ResolvedServicePricing> ResolvePricingAsync(
            long stylistId,
            long serviceManagementId,
            decimal basePrice,
            TimeSpan baseDuration,
            int baseDepositPercent,
            bool hasDynamicPricing,
            long bookingId,
            List<long>? optionValueIds)
        {
            optionValueIds = NormalizeOptionValueIds(optionValueIds);

            if (bookingId > 0)
            {
                optionValueIds = await _context.BookingServiceOptionValues
                    .AsNoTracking()
                    .Where(x => x.BookingID == bookingId && x.ServiceManagementID == serviceManagementId)
                    .Select(x => x.ServiceOptionValueID)
                    .Distinct()
                    .ToListAsync();
            }

            if (hasDynamicPricing && !optionValueIds.Any())
            {
                optionValueIds = await _context.StylistServicePriceVariants
                    .AsNoTracking()
                    .Where(x =>
                        x.StylistID == stylistId &&
                        x.ServiceManagementID == serviceManagementId &&
                        x.IsActive)
                    .SelectMany(x => x.OptionValues.Select(optionValue => optionValue.ServiceOptionValueID))
                    .Distinct()
                    .OrderBy(id => id)
                    .ToListAsync();
            }

            if (!hasDynamicPricing || !optionValueIds.Any())
            {
                return new ResolvedServicePricing(basePrice, baseDuration, baseDepositPercent, null, optionValueIds, await BuildOptionSummaryAsync(optionValueIds));
            }

            var variants = await _context.StylistServicePriceVariants
                .AsNoTracking()
                .Include(x => x.OptionValues)
                .Where(x => x.StylistID == stylistId &&
                            x.ServiceManagementID == serviceManagementId &&
                            x.IsActive)
                .ToListAsync();

            var selected = optionValueIds.OrderBy(x => x).ToList();
            var matchedVariant = variants.FirstOrDefault(x =>
                x.OptionValues.Select(ov => ov.ServiceOptionValueID).OrderBy(id => id).SequenceEqual(selected));

            if (matchedVariant == null)
            {
                return new ResolvedServicePricing(basePrice, baseDuration, baseDepositPercent, null, optionValueIds, await BuildOptionSummaryAsync(optionValueIds));
            }

            return new ResolvedServicePricing(
                matchedVariant.Price,
                matchedVariant.Duration,
                matchedVariant.DepositPercent,
                matchedVariant.ID,
                optionValueIds,
                await BuildOptionSummaryAsync(optionValueIds));
        }

        private async Task<string> BuildOptionSummaryAsync(List<long> optionValueIds)
        {
            if (optionValueIds == null || !optionValueIds.Any())
                return "";

            var optionValues = await _context.ServiceOptionValues
                .AsNoTracking()
                .Where(x => optionValueIds.Contains(x.ID))
                .Select(x => new
                {
                    x.ID,
                    x.ValueName,
                    x.ServiceOption.OptionName,
                    OptionSortOrder = x.ServiceOption.SortOrder,
                    ValueSortOrder = x.SortOrder
                })
                .ToListAsync();

            return string.Join("، ", optionValues
                .OrderBy(x => x.OptionSortOrder)
                .ThenBy(x => x.ValueSortOrder)
                .Select(x => $"{x.OptionName}: {x.ValueName}"));
        }

        private static List<long> NormalizeOptionValueIds(List<long>? optionValueIds)
        {
            return optionValueIds?
                .Where(x => x > 0)
                .Distinct()
                .OrderBy(x => x)
                .ToList() ?? new List<long>();
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

        private record ResolvedServicePricing(
            decimal Price,
            TimeSpan Duration,
            int DepositPercent,
            long? VariantId,
            List<long> OptionValueIds,
            string OptionSummary);

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
                      && (sd.StylistId == null || sd.StylistId <= 0 || sd.StylistId == stylistId)
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
                      && (cd.StylistId <= 0 || cd.StylistId == stylistId)
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
                       || ((da.StylistId == null || da.StylistId <= 0) && da.AdminId != null && da.AdminId > 0))
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
