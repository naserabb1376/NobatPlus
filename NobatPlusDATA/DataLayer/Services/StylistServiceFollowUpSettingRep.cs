using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class StylistServiceFollowUpSettingRep : IStylistServiceFollowUpSettingRep
    {
        private readonly NobatPlusContext _context;

        public StylistServiceFollowUpSettingRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddStylistServiceFollowUpSettingAsync(StylistServiceFollowUpSetting setting)
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

                await _context.StylistServiceFollowUpSettings.AddAsync(setting);
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

        public async Task<BitResultObject> EditStylistServiceFollowUpSettingAsync(StylistServiceFollowUpSetting setting)
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

                _context.StylistServiceFollowUpSettings.Update(setting);
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

        public async Task<BitResultObject> ExistStylistServiceFollowUpSettingAsync(long settingId)
        {
            var result = new BitResultObject();
            try
            {
                result.Status = await _context.StylistServiceFollowUpSettings.AsNoTracking().AnyAsync(x => x.ID == settingId);
                result.ID = settingId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<List<StylistServiceFollowUpSetting>> GetActiveStylistServiceFollowUpSettingsAsync(long stylistId, List<long> serviceManagementIds)
        {
            return await _context.StylistServiceFollowUpSettings
                .AsNoTracking()
                .Include(x => x.Stylist).ThenInclude(x => x.Person)
                .Include(x => x.ServiceManagement)
                .Where(x =>
                    x.IsActive &&
                    x.StylistID == stylistId &&
                    !x.StylistServicePriceVariantID.HasValue &&
                    serviceManagementIds.Contains(x.ServiceManagementID))
                .ToListAsync();
        }

        public async Task<ListResultObject<StylistServiceFollowUpSetting>> GetAllStylistServiceFollowUpSettingsAsync(long stylistId = 0, long serviceManagementId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            var results = new ListResultObject<StylistServiceFollowUpSetting>();
            try
            {
                var query = _context.StylistServiceFollowUpSettings
                    .AsNoTracking()
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.ServiceManagement)
                    .Include(x => x.StylistServicePriceVariant)
                    .AsQueryable();

                if (stylistId > 0)
                    query = query.Where(x => x.StylistID == stylistId);

                if (serviceManagementId > 0)
                    query = query.Where(x => x.ServiceManagementID == serviceManagementId);

                if (isActive >= 0)
                    query = query.Where(x => x.IsActive == (isActive == 1));

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.AfterCareMessageSettingKey) && x.AfterCareMessageSettingKey.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.RepairReminderMessageSettingKey) && x.RepairReminderMessageSettingKey.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                        (x.ServiceManagement != null && x.ServiceManagement.ServiceName.Contains(searchText)) ||
                        (x.Stylist != null && x.Stylist.StylistName.Contains(searchText)));
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

        public async Task<RowResultObject<StylistServiceFollowUpSetting>> GetStylistServiceFollowUpSettingByIdAsync(long settingId)
        {
            var result = new RowResultObject<StylistServiceFollowUpSetting>();
            try
            {
                result.Result = await _context.StylistServiceFollowUpSettings
                    .AsNoTracking()
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.ServiceManagement)
                    .Include(x => x.StylistServicePriceVariant)
                    .SingleOrDefaultAsync(x => x.ID == settingId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveStylistServiceFollowUpSettingAsync(long settingId)
        {
            var result = new BitResultObject();
            try
            {
                var row = await _context.StylistServiceFollowUpSettings.SingleOrDefaultAsync(x => x.ID == settingId);
                if (row == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "تنظیمات follow up خدمت یافت نشد";
                    return result;
                }

                _context.StylistServiceFollowUpSettings.Remove(row);
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

        private async Task<string> ValidateSettingAsync(StylistServiceFollowUpSetting setting, long excludedId = 0)
        {
            if (setting.StylistID <= 0)
                return "آرایشگر الزامی است";

            if (setting.ServiceManagementID <= 0)
                return "خدمت الزامی است";

            var stylistServiceExists = await _context.StylistServices
                .AsNoTracking()
                .AnyAsync(x => x.StylistID == setting.StylistID && x.ServiceManagementID == setting.ServiceManagementID);

            if (!stylistServiceExists)
                return "این خدمت برای آرایشگر انتخاب شده ثبت نشده است";

            if (setting.StylistServicePriceVariantID.HasValue &&
                !await _context.StylistServicePriceVariants.AsNoTracking().AnyAsync(x =>
                    x.ID == setting.StylistServicePriceVariantID.Value &&
                    x.StylistID == setting.StylistID &&
                    x.ServiceManagementID == setting.ServiceManagementID))
                return "قیمت متغیر انتخاب شده متعلق به این آرایشگر و خدمت نیست";

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

            var variantId = setting.StylistServicePriceVariantID.GetValueOrDefault();
            var duplicateExists = await _context.StylistServiceFollowUpSettings
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ID != excludedId &&
                    x.StylistID == setting.StylistID &&
                    x.ServiceManagementID == setting.ServiceManagementID &&
                    x.StylistServicePriceVariantID.GetValueOrDefault() == variantId);

            return duplicateExists ? "برای این آرایشگر و خدمت، تنظیمات follow up قبلا ثبت شده است" : "";
        }
    }
}
