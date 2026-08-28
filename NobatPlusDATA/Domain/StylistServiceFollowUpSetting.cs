using Domains;

namespace NobatPlusDATA.Domain
{
    public class StylistServiceFollowUpSetting : BaseEntity
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

        public Stylist Stylist { get; set; }
        public ServiceManagement ServiceManagement { get; set; }
        public StylistService StylistService { get; set; }
        public StylistServicePriceVariant? StylistServicePriceVariant { get; set; }
    }
}
