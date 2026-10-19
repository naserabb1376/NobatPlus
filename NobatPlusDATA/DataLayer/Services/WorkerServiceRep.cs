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
    public class WorkerServiceRep : IWorkerServiceRep
    {

        private NobatPlusContext _context;
        public WorkerServiceRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddWorkerServicesAsync(List<WorkerService> workerServices)
        {
            BitResultObject result = new BitResultObject();

            try
            {
                // گروه‌بندی بر اساس Worker
                var groupedByWorker = workerServices
                    .GroupBy(x => x.WorkerID)
                    .ToList();

                foreach (var workerGroup in groupedByWorker)
                {
                    var workerId = workerGroup.Key;

                    // سرویس‌های موجود این worker
                    var existingServices = await _context.WorkerServices
                        .AsNoTracking()
                        .Where(x => x.WorkerID == workerId)
                        .ToListAsync();

                    var existingServiceIds = existingServices
                        .Select(x => x.ServiceManagementID)
                        .ToHashSet();

                    var servicesToInsert = new List<WorkerService>();
                    var priceVariantsToInsert = new List<WorkerServicePriceVariant>();

                    foreach (var inputService in workerGroup)
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
                                        result.ErrorMessage = $"این اطلاعات (انجام دهنده خدمت: {inputService.WorkerID}, خدمت: {inputService.ServiceManagementID}) قبلا در سیسستم ثبت شده است";
                                        return result;
                                    }

                                    priceVariantsToInsert.AddRange(BuildPriceVariantsForInsert(inputService, workerId, service.ID));
                                }

                                continue;
                            }

                            // اگر parent است → قیمت و مدت صفر
                            bool isParent = service.ID != inputService.ServiceManagementID;

                            servicesToInsert.Add(new WorkerService
                            {
                                WorkerID = workerId,
                                ServiceManagementID = service.ID,
                                ServicePrice = isParent ? 0 : inputService.ServicePrice,
                                DepositPercent = isParent ? 0 : inputService.DepositPercent,
                                ServiceDuration = isParent ? TimeSpan.Zero : inputService.ServiceDuration,
                                HasDynamicPricing = !isParent && inputService.HasDynamicPricing,
                                PriceVariants = isParent
                                    ? new List<WorkerServicePriceVariant>()
                                    : BuildPriceVariantsForInsert(inputService, workerId, service.ID)
                            });

                            existingServiceIds.Add(service.ID);
                        }
                    }

                    var allPriceVariantsToInsert = servicesToInsert
                        .SelectMany(x => x.PriceVariants ?? new List<WorkerServicePriceVariant>())
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
                        await _context.WorkerServices.AddRangeAsync(servicesToInsert);

                    if (priceVariantsToInsert.Any())
                        await _context.WorkerServicePriceVariants.AddRangeAsync(priceVariantsToInsert);

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

        private static List<WorkerServicePriceVariant> BuildPriceVariantsForInsert(
            WorkerService source,
            long workerId,
            long serviceManagementId)
        {
            return source.PriceVariants?.Select(variant => new WorkerServicePriceVariant
            {
                CreateDate = variant.CreateDate ?? DateTime.Now.ToShamsi(),
                UpdateDate = variant.UpdateDate ?? DateTime.Now.ToShamsi(),
                Description = variant.Description,
                WorkerID = workerId,
                ServiceManagementID = serviceManagementId,
                Price = variant.Price,
                Duration = variant.Duration,
                DepositPercent = variant.DepositPercent,
                IsActive = variant.IsActive,
                OptionValueCombinationKey = WorkerServicePriceVariant.BuildOptionValueCombinationKey(
                    variant.OptionValues?.Select(x => x.ServiceOptionValueID)),
                OptionValues = variant.OptionValues?.Select(optionValue => new WorkerServicePriceVariantOptionValue
                {
                    ServiceOptionValueID = optionValue.ServiceOptionValueID
                }).ToList() ?? new List<WorkerServicePriceVariantOptionValue>()
            }).ToList() ?? new List<WorkerServicePriceVariant>();
        }


        public async Task<BitResultObject> EditWorkerServicesAsync(List<WorkerService> workerServices)
        {
            BitResultObject result = new BitResultObject();

            try
            {
                // گروه‌بندی بر اساس Worker
                var groupedByWorker = workerServices
                    .GroupBy(x => x.WorkerID)
                    .ToList();

                foreach (var workerGroup in groupedByWorker)
                {
                    var workerId = workerGroup.Key;
                    var servicesToReinsert = workerGroup.ToList();

                    // حذف همه سرویس‌های قبلی این worker
                    var oldItems = await _context.WorkerServices
                        .Include(x => x.PriceVariants)
                            .ThenInclude(x => x.OptionValues)
                        .Where(x => x.WorkerID == workerId)
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
                            new List<WorkerServicePriceVariant>();

                        if (!inputService.HasDynamicPricing)
                        {
                            requestedVariants = new List<WorkerServicePriceVariant>();
                        }
                        else if (!requestedVariants.Any())
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
                                    var combinationKey = WorkerServicePriceVariant.BuildOptionValueCombinationKey(
                                        variant.OptionValues?.Select(x => x.ServiceOptionValueID));
                                    var oldVariant = oldItem.PriceVariants.FirstOrDefault(x =>
                                        (variant.ID > 0 && x.ID == variant.ID) ||
                                        (x.OptionValueCombinationKey == combinationKey && x.BookingTagID == variant.BookingTagID));

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
                                .Where(x => !string.IsNullOrWhiteSpace(x.OptionValueCombinationKey))
                                .Select(x => $"{x.OptionValueCombinationKey}:{x.BookingTagID?.ToString() ?? "null"}")
                                .ToHashSet();

                            var untouchedOldVariants = oldItem.PriceVariants
                                .Where(oldVariant =>
                                    !requestedVariantIds.Contains(oldVariant.ID) &&
                                    !requestedCombinationKeys.Contains($"{oldVariant.OptionValueCombinationKey}:{oldVariant.BookingTagID?.ToString() ?? "null"}"))
                                .Select(variant => CloneVariantForReinsert(variant, now))
                                .ToList();

                            requestedVariants.AddRange(untouchedOldVariants);
                        }

                        inputService.PriceVariants = requestedVariants;
                    }

                    var requestedServiceIds = servicesToReinsert
                        .Select(x => x.ServiceManagementID)
                        .ToHashSet();

                    var itemsToReplace = oldItems
                        .Where(x => requestedServiceIds.Contains(x.ServiceManagementID))
                        .ToList();

                    if (itemsToReplace.Any())
                    {
                        await DetachVariantReferencesAsync(itemsToReplace
                            .SelectMany(x => x.PriceVariants ?? new List<WorkerServicePriceVariant>())
                            .Select(x => x.ID));
                        _context.WorkerServices.RemoveRange(itemsToReplace);
                        await _context.SaveChangesAsync();
                    }

                    // افزودن مجدد فقط سرویس‌های مربوط به همین worker
                    var addResult = await AddWorkerServicesAsync(servicesToReinsert);
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

        private static WorkerServicePriceVariant CloneVariantForReinsert(
            WorkerServicePriceVariant source,
            DateTime updateDate)
        {
            return new WorkerServicePriceVariant
            {
                CreateDate = source.CreateDate ?? updateDate,
                UpdateDate = updateDate,
                Description = source.Description,
                WorkerID = source.WorkerID,
                ServiceManagementID = source.ServiceManagementID,
                Price = source.Price,
                Duration = source.Duration,
                DepositPercent = source.DepositPercent,
                IsActive = source.IsActive,
                BookingTagID = source.BookingTagID,
                OptionValueCombinationKey = source.OptionValueCombinationKey,
                OptionValues = source.OptionValues
                    .Select(x => new WorkerServicePriceVariantOptionValue
                    {
                        ServiceOptionValueID = x.ServiceOptionValueID
                    })
                    .ToList()
            };
        }

        private static WorkerService CloneWorkerServiceForReinsert(
            WorkerService source,
            DateTime updateDate)
        {
            return new WorkerService
            {
                WorkerID = source.WorkerID,
                ServiceManagementID = source.ServiceManagementID,
                ServicePrice = source.ServicePrice,
                DepositPercent = source.DepositPercent,
                ServiceDuration = source.ServiceDuration,
                HasDynamicPricing = source.HasDynamicPricing,
                Description = source.Description,
                PriceVariants = source.PriceVariants?
                    .Select(variant => CloneVariantForReinsert(variant, updateDate))
                    .ToList() ?? new List<WorkerServicePriceVariant>()
            };
        }



        public async Task<BitResultObject> RemoveWorkerServicesAsync(List<WorkerService> workerServices)
        {
            BitResultObject result = new BitResultObject();

            try
            {
                // گروه‌بندی بر اساس Worker
                var groupedByWorker = workerServices
                    .GroupBy(x => x.WorkerID)
                    .ToList();

                foreach (var workerGroup in groupedByWorker)
                {
                    var workerId = workerGroup.Key;

                    var serviceIdsToRemove = new HashSet<long>();
                    var parentIdsToCheck = new List<long>();

                    foreach (var item in workerGroup)
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

                    var existingServiceIds = await _context.WorkerServices
                        .AsNoTracking()
                        .Where(x => x.WorkerID == workerId)
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

                    var itemsToRemove = await _context.WorkerServices
                        .Where(x =>
                            x.WorkerID == workerId &&
                            serviceIdsToRemove.Contains(x.ServiceManagementID))
                        .ToListAsync();

                    if (itemsToRemove.Any())
                    {
                        _context.WorkerServices.RemoveRange(itemsToRemove);
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

        public async Task<BitResultObject> RemoveWorkerServicesAsync(
      List<(long WorkerId, long ServiceManagementId, long? workerServicePriceVariantId)> workerServiceIds)
        {
            BitResultObject result = new BitResultObject();

            try
            {
                var fixedWorkerServices = new List<WorkerService>();

                foreach (var item in workerServiceIds)
                {
                    if (item.workerServicePriceVariantId.HasValue && item.workerServicePriceVariantId.Value > 0)
                    {
                        var dynamicRemoveResult = await RemoveDynamicWorkerServiceAsync(
                            item.WorkerId,
                            item.ServiceManagementId,
                            item.workerServicePriceVariantId.Value);

                        if (!dynamicRemoveResult.Status)
                            return dynamicRemoveResult;

                        continue;
                    }

                    fixedWorkerServices.Add(new WorkerService
                    {
                        WorkerID = item.WorkerId,
                        ServiceManagementID = item.ServiceManagementId
                    });
                }

                if (fixedWorkerServices.Any())
                {
                    result = await RemoveWorkerServicesAsync(fixedWorkerServices);
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

        private async Task<BitResultObject> RemoveDynamicWorkerServiceAsync(long workerId, long serviceManagementId, long workerServicePriceVariantId)
        {
            var result = new BitResultObject();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var variant = await _context.WorkerServicePriceVariants
                    .SingleOrDefaultAsync(x =>
                        x.ID == workerServicePriceVariantId &&
                        x.WorkerID == workerId &&
                        x.ServiceManagementID == serviceManagementId);

                if (variant == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "ردیف قیمت متغیر برای خدمات دهنده و خدمت ورودی پیدا نشد";
                    return result;
                }

                var optionValues = await _context.WorkerServicePriceVariantOptionValues
                    .Where(x => x.WorkerServicePriceVariantID == workerServicePriceVariantId)
                    .ToListAsync();

                if (optionValues.Any())
                    _context.WorkerServicePriceVariantOptionValues.RemoveRange(optionValues);

                await DetachVariantReferencesAsync(new[] { workerServicePriceVariantId });
                _context.WorkerServicePriceVariants.Remove(variant);
                await _context.SaveChangesAsync();

                var hasOtherVariants = await _context.WorkerServicePriceVariants
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkerID == workerId &&
                        x.ServiceManagementID == serviceManagementId);

                if (!hasOtherVariants)
                    await RemoveWorkerServiceAndUnusedParentsAsync(workerId, serviceManagementId);

                await transaction.CommitAsync();

                result.Status = true;
                result.ID = workerServicePriceVariantId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }

            return result;
        }

        private async Task RemoveWorkerServiceAndUnusedParentsAsync(long workerId, long serviceManagementId)
        {
            var existingServiceIds = await _context.WorkerServices
                .AsNoTracking()
                .Where(x => x.WorkerID == workerId)
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

            var workerServices = await _context.WorkerServices
                .Where(x =>
                    x.WorkerID == workerId &&
                    serviceIdsToRemove.Contains(x.ServiceManagementID))
                .ToListAsync();

            if (workerServices.Any())
            {
                        var variantIds = await _context.WorkerServicePriceVariants
                            .Where(x => x.WorkerID == workerId && serviceIdsToRemove.Contains(x.ServiceManagementID))
                            .Select(x => x.ID)
                            .ToListAsync();
                        await DetachVariantReferencesAsync(variantIds);
                _context.WorkerServices.RemoveRange(workerServices);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BitResultObject> ExistWorkerServiceAsync(long WorkerId, long ServiceManagementId)
        {
            BitResultObject result = new BitResultObject();
            try
            {
                result.Status = await _context.WorkerServices.AnyAsync(x => x.WorkerID == WorkerId && x.ServiceManagementID == ServiceManagementId);
                result.ID = ServiceManagementId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
         
        }

        public async Task<ListResultObject<WorkerServiceWithDiscountDto>> GetAllWorkerServicesAsync(
     long workerId = 0,
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
            var results = new ListResultObject<WorkerServiceWithDiscountDto>();

            try
            {
                var now = DateTime.Now.ToShamsi();

                var query = _context.WorkerServices
                    .AsNoTracking()
                    .AsQueryable();

                if (workerId > 0)
                {
                    query = query.Where(ss => ss.WorkerID == workerId);
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

                        (x.Worker.Specialty != null &&
                         x.Worker.Specialty.Contains(searchText)) ||

                          (x.Description != null &&
                         x.Description.Contains(searchText)) ||

                        (x.Worker.WorkerName != null &&
                         x.Worker.WorkerName.Contains(searchText)) ||

                        (x.Worker.Person.FirstName != null &&
                         x.Worker.Person.FirstName.Contains(searchText)) ||

                        (x.Worker.Person.LastName != null &&
                         x.Worker.Person.LastName.Contains(searchText))
                    );
                }

                optionValueIds = NormalizeOptionValueIds(optionValueIds);

                var baseQuery = query.Select(ss => new
                {
                    ss.WorkerID,
                    ss.ServiceManagementID,

                    ServiceTitle = ss.ServiceManagement.ServiceName,
                    ServiceDescription = ss.ServiceManagement.Description ?? "",

                    WorkshopName = ss.Worker.WorkerName,

                    FirstName = ss.Worker.Person.FirstName,
                    LastName = ss.Worker.Person.LastName,

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

                var finalList = new List<WorkerServiceWithDiscountDto>();

                foreach (var item in pageItems)
                {
                    var resolvedPricing = await ResolvePricingAsync(
                        item.WorkerID,
                        item.ServiceManagementID,
                        item.ServicePrice,
                        item.ServiceDuration,
                        item.DepositPercent,
                        item.HasDynamicPricing,
                        bookingId,
                        optionValueIds);

                    var discountPercent = await GetApplicableDiscountPercentsQuery(
         item.WorkerID,
         item.ServiceManagementID,
         customerId,
         discountId,
         now
     )
     .Select(x => (decimal?)x)
     .MaxAsync() ?? 0m;

                    var discountPercentDecimal = Math.Clamp(Convert.ToDecimal(discountPercent), 0m, 100m);
                    var priceVariants = item.HasDynamicPricing
                        ? await GetPriceVariantItemsAsync(item.WorkerID, item.ServiceManagementID, discountPercentDecimal)
                        : new List<WorkerServicePriceVariantPriceDto>();

                    var priceAfterDiscount =
                        resolvedPricing.Price * (1m - (discountPercentDecimal / 100m));

                    finalList.Add(new WorkerServiceWithDiscountDto
                    {
                        WorkerID = item.WorkerID,
                        ServiceManagementID = item.ServiceManagementID,
                        BookingID = bookingId,

                        ServiceTitle = item.ServiceTitle,
                        ServiceDescription = item.ServiceDescription,

                        WorkshopName = item.WorkshopName,
                        WorkerName = $"{item.FirstName} {item.LastName}".Trim(),

                        ServicePrice = resolvedPricing.Price,
                        ServiceDuration = resolvedPricing.Duration,
                        DepositPercent = resolvedPricing.DepositPercent,
                        HasDynamicPricing = item.HasDynamicPricing,
                        WorkerServicePriceVariantID = resolvedPricing.VariantId,
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

        public async Task<RowResultObject<WorkerServiceWithDiscountDto>> GetWorkerServiceByIdAsync(
            long workerId,
            long serviceManagementId,
            long customerId = 0,
            long discountId = 0,
            List<long>? optionValueIds = null
        )
        {
            var result = new RowResultObject<WorkerServiceWithDiscountDto>();

            try
            {
                var now = DateTime.Now.ToShamsi();

                var item = await _context.WorkerServices
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkerID == workerId &&
                        x.ServiceManagementID == serviceManagementId
                    )
                    .Select(ss => new
                    {
                        ss.WorkerID,
                        ss.ServiceManagementID,

                        ServiceTitle = ss.ServiceManagement.ServiceName,
                        ServiceDescription = ss.ServiceManagement.Description ?? "",

                        WorkshopName = ss.Worker.WorkerName,

                        FirstName = ss.Worker.Person.FirstName,
                        LastName = ss.Worker.Person.LastName,

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
                    item.WorkerID,
                    item.ServiceManagementID,
                    item.ServicePrice,
                    item.ServiceDuration,
                    item.DepositPercent,
                    item.HasDynamicPricing,
                    0,
                    optionValueIds);

                var discountPercent = await GetApplicableDiscountPercentsQuery(
                        item.WorkerID,
                        item.ServiceManagementID,
                        customerId,
                        discountId,
                        now
                    )
                    .Select(x => (decimal?)x)
                    .MaxAsync() ?? 0m;

                discountPercent = Math.Clamp(discountPercent, 0m, 100m);
                var priceVariants = item.HasDynamicPricing
                    ? await GetPriceVariantItemsAsync(item.WorkerID, item.ServiceManagementID, discountPercent)
                    : new List<WorkerServicePriceVariantPriceDto>();

                var returnedPrice = resolvedPricing.Price;
                var priceAfterDiscount =
                    returnedPrice * (1m - (discountPercent / 100m));

                result.Result = new WorkerServiceWithDiscountDto
                {
                    WorkerID = item.WorkerID,
                    ServiceManagementID = item.ServiceManagementID,

                    ServiceTitle = item.ServiceTitle,
                    ServiceDescription = item.ServiceDescription,

                    WorkshopName = item.WorkshopName,
                    WorkerName = $"{item.FirstName} {item.LastName}".Trim(),

                    ServicePrice = returnedPrice,
                    ServiceDuration = resolvedPricing.Duration,
                    DepositPercent = resolvedPricing.DepositPercent,
                    HasDynamicPricing = item.HasDynamicPricing,
                    WorkerServicePriceVariantID = resolvedPricing.VariantId,
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

        private async Task<List<WorkerServicePriceVariantPriceDto>> GetPriceVariantItemsAsync(
            long workerId,
            long serviceManagementId,
            decimal discountPercent)
        {
            discountPercent = Math.Clamp(discountPercent, 0m, 100m);

            var variants = await _context.WorkerServicePriceVariants
                .AsNoTracking()
                .Include(x => x.BookingTag)
                .Include(x => x.OptionValues)
                    .ThenInclude(x => x.ServiceOptionValue)
                    .ThenInclude(x => x.ServiceOption)
                .Where(x =>
                    x.WorkerID == workerId &&
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

                return new WorkerServicePriceVariantPriceDto
                {
                    WorkerServicePriceVariantID = variant.ID,
                    ServicePrice = variant.Price,
                    ServiceDuration = variant.Duration,
                    DepositPercent = variant.DepositPercent,
                    AppliedOptionValueIDs = optionValues.Select(x => x.ServiceOptionValueID).ToList(),
                    AppliedOptionSummary = string.Join("، ", optionValues.Select(x =>
                        $"{x.ServiceOptionValue.ServiceOption.OptionName}: {x.ServiceOptionValue.ValueName}")),
                    DiscountPercent = Convert.ToInt32(discountPercent),
                    PriceAfterDiscount = priceAfterDiscount,
                    BookingTagID = variant.BookingTagID,
                    BookingTagTitle = variant.BookingTag?.Title ?? "",
                    BookingTagColor = variant.BookingTag?.Color ?? ""
                };
            }).ToList();
        }

        private async Task<string> ValidatePriceVariantsAsync(IEnumerable<WorkerServicePriceVariant> variants)
        {
            var variantList = variants.ToList();
            foreach (var variant in variantList)
            {
                var optionValueIds = variant.OptionValues?
                    .Select(x => x.ServiceOptionValueID)
                    .Where(x => x > 0)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList() ?? new List<long>();

                variant.OptionValueCombinationKey = WorkerServicePriceVariant.BuildOptionValueCombinationKey(optionValueIds);

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

                if (!await _context.ServiceManagements.AsNoTracking()
                        .AnyAsync(x => x.ID == variant.ServiceManagementID))
                    return "خدمت انتخاب شده معتبر نیست";

                if (optionRows.Any(x => x.ServiceManagementID != variant.ServiceManagementID))
                    return "گزینه‌های انتخاب شده باید متعلق به همان خدمت باشند";

                if (optionRows.GroupBy(x => x.ServiceOptionID).Any(x => x.Count() > 1))
                    return "برای هر ویژگی فقط یک مقدار قابل انتخاب است";
            }

            var duplicateInput = variantList
                .GroupBy(x => new { x.WorkerID, x.ServiceManagementID, x.OptionValueCombinationKey, x.BookingTagID })
                .FirstOrDefault(x => x.Count() > 1);

                if (duplicateInput != null)
                    return "این خدمات قبلا ثبت شده است و تکراری است";

            foreach (var variant in variantList)
            {
                var exists = await _context.WorkerServicePriceVariants
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.ID != variant.ID &&
                        x.WorkerID == variant.WorkerID &&
                        x.ServiceManagementID == variant.ServiceManagementID &&
                        x.OptionValueCombinationKey == variant.OptionValueCombinationKey &&
                        x.BookingTagID == variant.BookingTagID);

                if (exists)
                    return "این خدمات قبلا ثبت شده است و تکراری است";

                if (variant.BookingTagID.HasValue && !await _context.BookingTags.AnyAsync(x => x.ID == variant.BookingTagID && x.WorkerID == variant.WorkerID && x.IsActive))
                    return "برچسب انتخاب‌شده برای این خدمات دهنده معتبر نیست";
            }

            return "";
        }

        private async Task<ResolvedServicePricing> ResolvePricingAsync(
            long workerId,
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
                optionValueIds = await _context.WorkerServicePriceVariants
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkerID == workerId &&
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

            var variants = await _context.WorkerServicePriceVariants
                .AsNoTracking()
                .Include(x => x.OptionValues)
                .Where(x => x.WorkerID == workerId &&
                            x.ServiceManagementID == serviceManagementId &&
                            x.IsActive && x.BookingTagID == null)
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

        private async Task DetachVariantReferencesAsync(IEnumerable<long> variantIds)
        {
            var ids = variantIds.Where(x => x > 0).Distinct().ToList();
            if (!ids.Any())
                return;

            var scheduleBlocks = await _context.WorkerScheduleBlocks
                .Where(x => x.WorkerServicePriceVariantID.HasValue && ids.Contains(x.WorkerServicePriceVariantID.Value))
                .ToListAsync();
            var bookingServices = await _context.BookingServices
                .Where(x => x.WorkerServicePriceVariantID.HasValue && ids.Contains(x.WorkerServicePriceVariantID.Value))
                .ToListAsync();
            var paymentDetails = await _context.PaymentDetails
                .Where(x => x.WorkerServicePriceVariantID.HasValue && ids.Contains(x.WorkerServicePriceVariantID.Value))
                .ToListAsync();
            var followUpSettings = await _context.WorkerServiceFollowUpSettings
                .Where(x => x.WorkerServicePriceVariantID.HasValue && ids.Contains(x.WorkerServicePriceVariantID.Value))
                .ToListAsync();
            var scheduledMessages = await _context.BookingScheduledMessages
                .Where(x => x.WorkerServicePriceVariantID.HasValue && ids.Contains(x.WorkerServicePriceVariantID.Value))
                .ToListAsync();

            foreach (var row in scheduleBlocks) row.WorkerServicePriceVariantID = null;
            foreach (var row in bookingServices) row.WorkerServicePriceVariantID = null;
            foreach (var row in paymentDetails) row.WorkerServicePriceVariantID = null;
            foreach (var row in followUpSettings) row.WorkerServicePriceVariantID = null;
            foreach (var row in scheduledMessages) row.WorkerServicePriceVariantID = null;

            if (scheduleBlocks.Any() || bookingServices.Any() || paymentDetails.Any() ||
                followUpSettings.Any() || scheduledMessages.Any())
            {
                await _context.SaveChangesAsync();
            }
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
    long workerId,
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
                      && (sd.WorkerId == null || sd.WorkerId <= 0 || sd.WorkerId == workerId)
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
                      && (cd.WorkerId <= 0 || cd.WorkerId == workerId)
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
                where (da.WorkerId == workerId
                       // اگر می‌خوای AdminId هم "عمومی" حساب شود:
                       || ((da.WorkerId == null || da.WorkerId <= 0) && da.AdminId != null && da.AdminId > 0))
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
