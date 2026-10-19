using NobatPlusAPI.Models.Public;

namespace NobatPlusAPI.Models.BookingScheduledMessage
{
    public class GetBookingScheduledMessageListRequestBody : GetListRequestBody
    {
        public long BookingID { get; set; } = 0;
        public long CustomerID { get; set; } = 0;
        public long WorkerID { get; set; } = 0;
        public long ServiceManagementID { get; set; } = 0;
        public int MessageType { get; set; } = 0;
        public int Status { get; set; } = -1;
        public int IsActive { get; set; } = -1;
    }

    public class AddEditBookingScheduledMessageRequestBody
    {
        public long ID { get; set; } = 0;
        public long BookingID { get; set; }
        public long WorkerID { get; set; }
        public long CustomerID { get; set; }
        public long? ServiceManagementID { get; set; }
        public long? WorkerServiceFollowUpSettingID { get; set; }
        public long? WorkerServicePriceVariantID { get; set; }
        public byte MessageType { get; set; }
        public string MessageText { get; set; } = "";
        public DateTime ScheduledAt { get; set; }
        public byte Status { get; set; }
        public DateTime? SentAt { get; set; }
        public int RetryCount { get; set; }
        public string? ProviderMessageID { get; set; }
        public string? ErrorMessage { get; set; }
        public string? HangfireJobID { get; set; }
        public long? SMSMessageID { get; set; }
        public long? NotificationID { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }

    public class ScheduleBookingMessagesRequestBody
    {
        public long BookingID { get; set; }
    }
}
