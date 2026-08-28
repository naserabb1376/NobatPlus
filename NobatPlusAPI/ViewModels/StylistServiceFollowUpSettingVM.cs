using Domains;

namespace NobatPlusAPI.ViewModels
{
    public class StylistServiceFollowUpSettingVM : BaseEntity
    {
        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }
        public long? StylistServicePriceVariantID { get; set; }
        public bool RepairEnabled { get; set; }
        public int? RepairAfterDays { get; set; }
        public bool RepairReminderEnabled { get; set; }
        public int? RepairReminderBeforeDays { get; set; }
        public string? RepairReminderMessageSettingKey { get; set; }
        public bool AfterCareEnabled { get; set; }
        public int? AfterCareDelayMinutes { get; set; }
        public string? AfterCareMessageSettingKey { get; set; }
        public bool IsActive { get; set; }
        public string StylistName { get; set; } = "";
        public string SalonName { get; set; } = "";
        public string ServiceName { get; set; } = "";
    }
}
