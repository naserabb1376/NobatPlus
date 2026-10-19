using Domains;

namespace NobatPlusAPI.ViewModels
{
    public class WorkerServiceFollowUpSettingVM : BaseEntity
    {
        public long WorkerID { get; set; }
        public long ServiceManagementID { get; set; }
        public long? WorkerServicePriceVariantID { get; set; }
        public bool RepairEnabled { get; set; }
        public int? RepairAfterDays { get; set; }
        public bool RepairReminderEnabled { get; set; }
        public int? RepairReminderBeforeDays { get; set; }
        public string? RepairReminderMessageSettingKey { get; set; }
        public bool AfterCareEnabled { get; set; }
        public int? AfterCareDelayMinutes { get; set; }
        public string? AfterCareMessageSettingKey { get; set; }
        public string? AfterCareInstructions { get; set; }
        public bool IsActive { get; set; }
        public string WorkerName { get; set; } = "";
        public string WorkshopName { get; set; } = "";
        public string ServiceName { get; set; } = "";
    }
}
