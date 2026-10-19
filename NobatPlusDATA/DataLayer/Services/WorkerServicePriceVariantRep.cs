using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class WorkerServicePriceVariantRep : IWorkerServicePriceVariantRep
    {
        private readonly NobatPlusContext _context;

        public WorkerServicePriceVariantRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddWorkerServicePriceVariantAsync(WorkerServicePriceVariant workerServicePriceVariant)
        {
            var result = new BitResultObject();
            try
            {
                var validationError = await ValidateVariantAsync(workerServicePriceVariant);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                await _context.WorkerServicePriceVariants.AddAsync(workerServicePriceVariant);
                await _context.SaveChangesAsync();
                result.ID = workerServicePriceVariant.ID;
                _context.Entry(workerServicePriceVariant).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditWorkerServicePriceVariantAsync(WorkerServicePriceVariant workerServicePriceVariant)
        {
            var result = new BitResultObject();
            try
            {
                var validationError = await ValidateVariantAsync(workerServicePriceVariant, workerServicePriceVariant.ID);
                if (!string.IsNullOrEmpty(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                var oldOptions = await _context.WorkerServicePriceVariantOptionValues
                    .Where(x => x.WorkerServicePriceVariantID == workerServicePriceVariant.ID)
                    .ToListAsync();

                _context.WorkerServicePriceVariantOptionValues.RemoveRange(oldOptions);
                _context.WorkerServicePriceVariants.Update(workerServicePriceVariant);
                await _context.SaveChangesAsync();
                result.ID = workerServicePriceVariant.ID;
                _context.Entry(workerServicePriceVariant).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistWorkerServicePriceVariantAsync(long workerServicePriceVariantId)
        {
            var result = new BitResultObject();
            try
            {
                result.Status = await _context.WorkerServicePriceVariants.AsNoTracking().AnyAsync(x => x.ID == workerServicePriceVariantId);
                result.ID = workerServicePriceVariantId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<WorkerServicePriceVariant>> GetAllWorkerServicePriceVariantsAsync(long workerId = 0, long serviceManagementId = 0, int isActive = -1, bool onlyLeafServices = false, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            var results = new ListResultObject<WorkerServicePriceVariant>();
            try
            {
                var query = _context.WorkerServicePriceVariants
                    .AsNoTracking()
                    .Include(x => x.BookingTag)
                    .Include(x => x.OptionValues)
                    .ThenInclude(x => x.ServiceOptionValue)
                    .ThenInclude(x => x.ServiceOption)
                    .AsQueryable();

                if (workerId > 0)
                    query = query.Where(x => x.WorkerID == workerId);

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

        public async Task<RowResultObject<WorkerServicePriceVariant>> GetWorkerServicePriceVariantByIdAsync(long workerServicePriceVariantId)
        {
            var result = new RowResultObject<WorkerServicePriceVariant>();
            try
            {
                result.Result = await _context.WorkerServicePriceVariants
                    .AsNoTracking()
                    .Include(x => x.BookingTag)
                    .Include(x => x.OptionValues)
                    .ThenInclude(x => x.ServiceOptionValue)
                    .ThenInclude(x => x.ServiceOption)
                    .SingleOrDefaultAsync(x => x.ID == workerServicePriceVariantId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveWorkerServicePriceVariantAsync(long workerServicePriceVariantId)
        {
            var result = new BitResultObject();
            try
            {
                var variant = await _context.WorkerServicePriceVariants.SingleOrDefaultAsync(x => x.ID == workerServicePriceVariantId);
                if (variant == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "Worker service price variant not found";
                    return result;
                }

                await DetachVariantReferencesAsync(workerServicePriceVariantId);
                _context.WorkerServicePriceVariants.Remove(variant);
                await _context.SaveChangesAsync();
                result.ID = workerServicePriceVariantId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        private async Task DetachVariantReferencesAsync(long variantId)
        {
            var scheduleBlocks = await _context.WorkerScheduleBlocks
                .Where(x => x.WorkerServicePriceVariantID == variantId)
                .ToListAsync();
            var bookingServices = await _context.BookingServices
                .Where(x => x.WorkerServicePriceVariantID == variantId)
                .ToListAsync();
            var paymentDetails = await _context.PaymentDetails
                .Where(x => x.WorkerServicePriceVariantID == variantId)
                .ToListAsync();
            var followUpSettings = await _context.WorkerServiceFollowUpSettings
                .Where(x => x.WorkerServicePriceVariantID == variantId)
                .ToListAsync();
            var scheduledMessages = await _context.BookingScheduledMessages
                .Where(x => x.WorkerServicePriceVariantID == variantId)
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

        private async Task<string> ValidateVariantAsync(WorkerServicePriceVariant variant, long excludedVariantId = 0)
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

            var duplicateExists = await _context.WorkerServicePriceVariants
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ID != excludedVariantId &&
                    x.WorkerID == variant.WorkerID &&
                    x.ServiceManagementID == variant.ServiceManagementID &&
                    x.OptionValueCombinationKey == variant.OptionValueCombinationKey &&
                    x.BookingTagID == variant.BookingTagID);

            if (variant.BookingTagID.HasValue && !await _context.BookingTags
                    .AnyAsync(x => x.ID == variant.BookingTagID.Value && x.WorkerID == variant.WorkerID && x.IsActive))
                return "برچسب انتخاب‌شده برای این خدمات دهنده معتبر نیست";

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
