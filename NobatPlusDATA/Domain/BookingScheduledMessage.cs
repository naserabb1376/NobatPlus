using Domains;

namespace NobatPlusDATA.Domain
{
    public class BookingScheduledMessage : BaseEntity
    {
        public long BookingID { get; set; }
        public long StylistID { get; set; }
        public long CustomerID { get; set; }
        public long? ServiceManagementID { get; set; }
        public long? StylistServiceFollowUpSettingID { get; set; }
        public long? StylistServicePriceVariantID { get; set; }
        public byte MessageType { get; set; }
        public string MessageText { get; set; } = "";
        public string? AfterCareInstructions { get; set; }
        public DateTime ScheduledAt { get; set; }
        public byte Status { get; set; }
        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; }
        public string? ProviderMessageID { get; set; }
        public string? ErrorMessage { get; set; }
        public string? HangfireJobID { get; set; }
        public long? SMSMessageID { get; set; }
        public long? NotificationID { get; set; }

        public Booking Booking { get; set; }
        public Stylist Stylist { get; set; }
        public Customer Customer { get; set; }
        public ServiceManagement? ServiceManagement { get; set; }
        public StylistServiceFollowUpSetting? StylistServiceFollowUpSetting { get; set; }
        public StylistServicePriceVariant? StylistServicePriceVariant { get; set; }
        public SMSMessage? SMSMessage { get; set; }
        public Notification? Notification { get; set; }
    }

    public enum BookingScheduledMessageType : byte
    {
        AfterCare = 1,
        RepairReminder = 2
    }

    public enum BookingScheduledMessageStatus : byte
    {
        Pending = 0,
        Sent = 1,
        Failed = 2,
        Cancelled = 3
    }
}
