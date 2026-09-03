using Domains;

namespace NobatPlusAPI.ViewModels
{
    public class BookingScheduledMessageVM : BaseEntity
    {
        public long BookingID { get; set; }
        public long StylistID { get; set; }
        public long CustomerID { get; set; }
        public long? ServiceManagementID { get; set; }
        public long? StylistServiceFollowUpSettingID { get; set; }
        public long? StylistServicePriceVariantID { get; set; }
        public byte MessageType { get; set; }
        public string MessageTypeTitle { get; set; } = "";
        public string MessageText { get; set; } = "";
        public string? AfterCareInstructions { get; set; }
        public DateTime ScheduledAt { get; set; }
        public byte Status { get; set; }
        public string StatusTitle { get; set; } = "";
        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; }
        public string? ProviderMessageID { get; set; }
        public string? ErrorMessage { get; set; }
        public string? HangfireJobID { get; set; }
        public long? SMSMessageID { get; set; }
        public long? NotificationID { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhoneNumber { get; set; } = "";
        public string StylistName { get; set; } = "";
        public string SalonName { get; set; } = "";
        public string ServiceName { get; set; } = "";
        public bool FollowUpSettingIsActive { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
