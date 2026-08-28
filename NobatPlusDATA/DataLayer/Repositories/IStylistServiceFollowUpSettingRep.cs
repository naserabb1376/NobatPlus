using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistServiceFollowUpSettingRep
    {
        Task<ListResultObject<StylistServiceFollowUpSetting>> GetAllStylistServiceFollowUpSettingsAsync(long stylistId = 0, long serviceManagementId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<StylistServiceFollowUpSetting>> GetStylistServiceFollowUpSettingByIdAsync(long settingId);
        Task<List<StylistServiceFollowUpSetting>> GetActiveStylistServiceFollowUpSettingsAsync(long stylistId, List<long> serviceManagementIds);
        Task<BitResultObject> AddStylistServiceFollowUpSettingAsync(StylistServiceFollowUpSetting setting);
        Task<BitResultObject> EditStylistServiceFollowUpSettingAsync(StylistServiceFollowUpSetting setting);
        Task<BitResultObject> RemoveStylistServiceFollowUpSettingAsync(long settingId);
        Task<BitResultObject> ExistStylistServiceFollowUpSettingAsync(long settingId);
    }
}
