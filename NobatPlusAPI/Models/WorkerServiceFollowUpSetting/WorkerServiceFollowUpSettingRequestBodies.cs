using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.WorkerServiceFollowUpSetting
{
    public class GetWorkerServiceFollowUpSettingListRequestBody : GetListRequestBody
    {
        public long WorkerID { get; set; } = 0;
        public long ServiceManagementID { get; set; } = 0;
        public int IsActive { get; set; } = -1;
    }

    public class AddEditWorkerServiceFollowUpSettingRequestBody
    {
        public long ID { get; set; } = 0;

        [Required(ErrorMessage = "لطفا خدمات دهنده را وارد کنید")]
        public long WorkerID { get; set; }

        [Required(ErrorMessage = "لطفا خدمت را وارد کنید")]
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
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }
}
