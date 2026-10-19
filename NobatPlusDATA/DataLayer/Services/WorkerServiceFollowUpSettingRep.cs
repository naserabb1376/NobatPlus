using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class WorkerServiceFollowUpSettingRep : IWorkerServiceFollowUpSettingRep
    {
        private readonly NobatPlusContext _context;

        public WorkerServiceFollowUpSettingRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddWorkerServiceFollowUpSettingAsync(WorkerServiceFollowUpSetting setting)
        {
            var result = new BitResultObject();
            try
            {
                var validationError = await ValidateSettingAsync(setting);
                if (!string.IsNullOrWhiteSpace(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                await _context.WorkerServiceFollowUpSettings.AddAsync(setting);
                await _context.SaveChangesAsync();
                result.ID = setting.ID;
                _context.Entry(setting).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditWorkerServiceFollowUpSettingAsync(WorkerServiceFollowUpSetting setting)
        {
            var result = new BitResultObject();
            try
            {
                var validationError = await ValidateSettingAsync(setting, setting.ID);
                if (!string.IsNullOrWhiteSpace(validationError))
                {
                    result.Status = false;
                    result.ErrorMessage = validationError;
                    return result;
                }

                _context.WorkerServiceFollowUpSettings.Update(setting);
                await _context.SaveChangesAsync();
                result.ID = setting.ID;
                _context.Entry(setting).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistWorkerServiceFollowUpSettingAsync(long settingId)
        {
            var result = new BitResultObject();
            try
            {
                result.Status = await _context.WorkerServiceFollowUpSettings.AsNoTracking().AnyAsync(x => x.ID == settingId);
                result.ID = settingId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<List<WorkerServiceFollowUpSetting>> GetActiveWorkerServiceFollowUpSettingsAsync(long workerId, List<long> serviceManagementIds)
        {
            return await _context.WorkerServiceFollowUpSettings
                .AsNoTracking()
                .Include(x => x.Worker).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement)
                .Where(x =>
                    x.IsActive &&
                    x.WorkerID == workerId &&
                    !x.WorkerServicePriceVariantID.HasValue &&
                    serviceManagementIds.Contains(x.ServiceManagementID))
                .ToListAsync();
        }

        public async Task<ListResultObject<WorkerServiceFollowUpSetting>> GetAllWorkerServiceFollowUpSettingsAsync(long workerId = 0, long serviceManagementId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            var results = new ListResultObject<WorkerServiceFollowUpSetting>();
            try
            {
                var query = _context.WorkerServiceFollowUpSettings
                    .AsNoTracking()
                    .Include(x => x.Worker).ThenInclude(x => x.Person)
                    .Include(x => x.ServiceManagement)
                    .Include(x => x.WorkerServicePriceVariant)
                    .AsQueryable();

                if (workerId > 0)
                    query = query.Where(x => x.WorkerID == workerId);

                if (serviceManagementId > 0)
                    query = query.Where(x => x.ServiceManagementID == serviceManagementId);

                if (isActive >= 0)
                    query = query.Where(x => x.IsActive == (isActive == 1));

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.AfterCareMessageSettingKey) && x.AfterCareMessageSettingKey.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.AfterCareInstructions) && x.AfterCareInstructions.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.RepairReminderMessageSettingKey) && x.RepairReminderMessageSettingKey.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                        (x.ServiceManagement != null && x.ServiceManagement.ServiceName.Contains(searchText)) ||
                        (x.Worker != null && x.Worker.WorkerName.Contains(searchText)));
                }

                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query
                    .OrderByDescending(x => x.CreateDate)
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

        public async Task<RowResultObject<WorkerServiceFollowUpSetting>> GetWorkerServiceFollowUpSettingByIdAsync(long settingId)
        {
            var result = new RowResultObject<WorkerServiceFollowUpSetting>();
            try
            {
                result.Result = await _context.WorkerServiceFollowUpSettings
                    .AsNoTracking()
                    .Include(x => x.Worker).ThenInclude(x => x.Person)
                    .Include(x => x.ServiceManagement)
                    .Include(x => x.WorkerServicePriceVariant)
                    .SingleOrDefaultAsync(x => x.ID == settingId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveWorkerServiceFollowUpSettingAsync(long settingId)
        {
            var result = new BitResultObject();
            try
            {
                var row = await _context.WorkerServiceFollowUpSettings.SingleOrDefaultAsync(x => x.ID == settingId);
                if (row == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "تنظیمات follow up خدمت یافت نشد";
                    return result;
                }

                _context.WorkerServiceFollowUpSettings.Remove(row);
                await _context.SaveChangesAsync();
                result.ID = settingId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        private async Task<string> ValidateSettingAsync(WorkerServiceFollowUpSetting setting, long excludedId = 0)
        {
            if (setting.WorkerID <= 0)
                return "خدمات دهنده الزامی است";

            if (setting.ServiceManagementID <= 0)
                return "خدمت الزامی است";

            var workerServiceExists = await _context.WorkerServices
                .AsNoTracking()
                .AnyAsync(x => x.WorkerID == setting.WorkerID && x.ServiceManagementID == setting.ServiceManagementID);

            if (!workerServiceExists)
                return "این خدمت برای خدمات دهنده انتخاب شده ثبت نشده است";

            if (setting.WorkerServicePriceVariantID.HasValue &&
                !await _context.WorkerServicePriceVariants.AsNoTracking().AnyAsync(x =>
                    x.ID == setting.WorkerServicePriceVariantID.Value &&
                    x.WorkerID == setting.WorkerID &&
                    x.ServiceManagementID == setting.ServiceManagementID))
                return "قیمت متغیر انتخاب شده متعلق به این خدمات دهنده و خدمت نیست";

            if (setting.AfterCareEnabled)
            {
                if (!setting.AfterCareDelayMinutes.HasValue || setting.AfterCareDelayMinutes.Value < 0)
                    return "زمان ارسال پیام مراقبت باید بر حسب دقیقه وارد شود";

                if (string.IsNullOrWhiteSpace(setting.AfterCareMessageSettingKey))
                    return "کلید الگوی پیام مراقبت در Settings الزامی است";
            }

            if (setting.RepairEnabled)
            {
                if (!setting.RepairAfterDays.HasValue || setting.RepairAfterDays.Value <= 0)
                    return "تعداد روز پیشنهادی ترمیم باید بزرگتر از صفر باشد";

                if (setting.RepairReminderEnabled)
                {
                    if (!setting.RepairReminderBeforeDays.HasValue || setting.RepairReminderBeforeDays.Value < 0)
                        return "تعداد روز قبل از ترمیم برای یادآوری معتبر نیست";

                    if (string.IsNullOrWhiteSpace(setting.RepairReminderMessageSettingKey))
                        return "کلید الگوی پیام یادآوری ترمیم در Settings الزامی است";
                }
            }

            var variantId = setting.WorkerServicePriceVariantID.GetValueOrDefault();
            var duplicateExists = await _context.WorkerServiceFollowUpSettings
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ID != excludedId &&
                    x.WorkerID == setting.WorkerID &&
                    x.ServiceManagementID == setting.ServiceManagementID &&
                    x.WorkerServicePriceVariantID.GetValueOrDefault() == variantId);

            return duplicateExists ? "برای این خدمات دهنده و خدمت، تنظیمات follow up قبلا ثبت شده است" : "";
        }
    }
}
