using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IBookingScheduledMessageRep
    {
        Task<ListResultObject<BookingScheduledMessage>> GetAllBookingScheduledMessagesAsync(long bookingId = 0, long customerId = 0, long stylistId = 0, long serviceManagementId = 0, int messageType = 0, int status = -1, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<BookingScheduledMessage>> GetBookingScheduledMessageByIdAsync(long messageId);
        Task<BitResultObject> AddBookingScheduledMessageAsync(BookingScheduledMessage message);
        Task<BitResultObject> EditBookingScheduledMessageAsync(BookingScheduledMessage message);
        Task<BitResultObject> SetHangfireJobIdAsync(long messageId, string hangfireJobId);
        Task<BitResultObject> MarkBookingScheduledMessageAsync(long messageId, BookingScheduledMessageStatus status, DateTime? sentAt, bool incrementRetry, long? smsMessageId = null, long? notificationId = null, string? providerMessageId = null, string? errorMessage = null);
        Task<BitResultObject> CancelPendingMessagesForBookingAsync(long bookingId);
        Task<BitResultObject> RemoveBookingScheduledMessageAsync(long messageId);
        Task<BitResultObject> ExistBookingScheduledMessageAsync(long messageId);
    }
}
