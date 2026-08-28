using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class BookingScheduledMessageRep : IBookingScheduledMessageRep
    {
        private readonly NobatPlusContext _context;

        public BookingScheduledMessageRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddBookingScheduledMessageAsync(BookingScheduledMessage message)
        {
            var result = new BitResultObject();
            try
            {
                var exists = await _context.BookingScheduledMessages.AsNoTracking().AnyAsync(x =>
                    x.BookingID == message.BookingID &&
                    x.ServiceManagementID == message.ServiceManagementID &&
                    x.StylistServiceFollowUpSettingID == message.StylistServiceFollowUpSettingID &&
                    x.StylistServicePriceVariantID == message.StylistServicePriceVariantID &&
                    x.MessageType == message.MessageType &&
                    x.ScheduledAt == message.ScheduledAt &&
                    x.MessageText == message.MessageText);

                if (exists)
                {
                    result.Status = false;
                    result.ErrorMessage = "این پیام زمان‌بندی‌شده قبلا ثبت شده است";
                    return result;
                }

                await _context.BookingScheduledMessages.AddAsync(message);
                await _context.SaveChangesAsync();
                result.ID = message.ID;
                _context.Entry(message).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> CancelPendingMessagesForBookingAsync(long bookingId)
        {
            var result = new BitResultObject();
            try
            {
                var rows = await _context.BookingScheduledMessages
                    .Where(x => x.BookingID == bookingId && x.Status == (byte)BookingScheduledMessageStatus.Pending)
                    .ToListAsync();

                foreach (var row in rows)
                {
                    row.Status = (byte)BookingScheduledMessageStatus.Cancelled;
                    row.IsActive = false;
                    row.UpdateDate = DateTime.Now.ToShamsi();
                }

                await _context.SaveChangesAsync();
                result.ID = bookingId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> EditBookingScheduledMessageAsync(BookingScheduledMessage message)
        {
            var result = new BitResultObject();
            try
            {
                _context.BookingScheduledMessages.Update(message);
                await _context.SaveChangesAsync();
                result.ID = message.ID;
                _context.Entry(message).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> ExistBookingScheduledMessageAsync(long messageId)
        {
            var result = new BitResultObject();
            try
            {
                result.Status = await _context.BookingScheduledMessages.AsNoTracking().AnyAsync(x => x.ID == messageId);
                result.ID = messageId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<BookingScheduledMessage>> GetAllBookingScheduledMessagesAsync(long bookingId = 0, long customerId = 0, long stylistId = 0, long serviceManagementId = 0, int messageType = 0, int status = -1, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "")
        {
            var results = new ListResultObject<BookingScheduledMessage>();
            try
            {
                var query = _context.BookingScheduledMessages
                    .AsNoTracking()
                    .Include(x => x.Booking)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.ServiceManagement)
                    .Include(x => x.StylistServiceFollowUpSetting)
                    .Include(x => x.StylistServicePriceVariant)
                    .AsQueryable();

                if (bookingId > 0)
                    query = query.Where(x => x.BookingID == bookingId);

                if (customerId > 0)
                    query = query.Where(x => x.CustomerID == customerId);

                if (stylistId > 0)
                    query = query.Where(x => x.StylistID == stylistId);

                if (serviceManagementId > 0)
                    query = query.Where(x => x.ServiceManagementID == serviceManagementId);

                if (messageType > 0)
                    query = query.Where(x => x.MessageType == messageType);

                if (status >= 0)
                    query = query.Where(x => x.Status == status);

                if (isActive >= 0)
                    query = query.Where(x => x.IsActive == (isActive == 1));

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        (!string.IsNullOrEmpty(x.MessageText) && x.MessageText.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(searchText)) ||
                        (!string.IsNullOrEmpty(x.ErrorMessage) && x.ErrorMessage.Contains(searchText)) ||
                        (x.ServiceManagement != null && x.ServiceManagement.ServiceName.Contains(searchText)));
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

        public async Task<RowResultObject<BookingScheduledMessage>> GetBookingScheduledMessageByIdAsync(long messageId)
        {
            var result = new RowResultObject<BookingScheduledMessage>();
            try
            {
                result.Result = await _context.BookingScheduledMessages
                    .AsNoTracking()
                    .Include(x => x.Booking)
                    .Include(x => x.Customer).ThenInclude(x => x.Person)
                    .Include(x => x.Stylist).ThenInclude(x => x.Person)
                    .Include(x => x.ServiceManagement)
                    .Include(x => x.StylistServiceFollowUpSetting)
                    .Include(x => x.StylistServicePriceVariant)
                    .SingleOrDefaultAsync(x => x.ID == messageId);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> MarkBookingScheduledMessageAsync(long messageId, BookingScheduledMessageStatus status, DateTime? sentAt, bool incrementRetry, long? smsMessageId = null, long? notificationId = null, string? providerMessageId = null, string? errorMessage = null)
        {
            var result = new BitResultObject();
            try
            {
                var row = await _context.BookingScheduledMessages.SingleOrDefaultAsync(x => x.ID == messageId);
                if (row == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "پیام زمان‌بندی‌شده یافت نشد";
                    return result;
                }

                row.UpdateDate = DateTime.Now.ToShamsi();
                row.Status = (byte)status;
                if (status == BookingScheduledMessageStatus.Cancelled)
                    row.IsActive = false;
                row.SentAt = sentAt;
                row.SMSMessageID = smsMessageId ?? row.SMSMessageID;
                row.NotificationID = notificationId ?? row.NotificationID;
                row.ProviderMessageID = providerMessageId ?? row.ProviderMessageID;
                row.ErrorMessage = errorMessage ?? "";
                if (incrementRetry)
                    row.RetryCount += 1;

                await _context.SaveChangesAsync();
                result.ID = row.ID;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> RemoveBookingScheduledMessageAsync(long messageId)
        {
            var result = new BitResultObject();
            try
            {
                var row = await _context.BookingScheduledMessages.SingleOrDefaultAsync(x => x.ID == messageId);
                if (row == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "پیام زمان‌بندی‌شده یافت نشد";
                    return result;
                }

                _context.BookingScheduledMessages.Remove(row);
                await _context.SaveChangesAsync();
                result.ID = messageId;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<BitResultObject> SetHangfireJobIdAsync(long messageId, string hangfireJobId)
        {
            var result = new BitResultObject();
            try
            {
                var row = await _context.BookingScheduledMessages.SingleOrDefaultAsync(x => x.ID == messageId);
                if (row == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "پیام زمان‌بندی‌شده یافت نشد";
                    return result;
                }

                row.UpdateDate = DateTime.Now.ToShamsi();
                row.HangfireJobID = hangfireJobId;
                await _context.SaveChangesAsync();
                result.ID = row.ID;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }
    }
}
