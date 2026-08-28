using AutoMapper;
using Domain;
using Domains;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.BookingScheduledMessage;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusAPI.ViewModels;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusAPI.Controllers
{
    [Route("BookingScheduledMessage")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class BookingScheduledMessageController : ControllerBase
    {
        private readonly IBookingScheduledMessageRep _bookingScheduledMessageRep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public BookingScheduledMessageController(IBookingScheduledMessageRep bookingScheduledMessageRep, ILogRep logRep, IMapper mapper)
        {
            _bookingScheduledMessageRep = bookingScheduledMessageRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllBookingScheduledMessages_Base")]
        public async Task<ActionResult<ListResultObject<BookingScheduledMessageVM>>> GetAllBookingScheduledMessages_Base(GetBookingScheduledMessageListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _bookingScheduledMessageRep.GetAllBookingScheduledMessagesAsync(
                requestBody.BookingID,
                requestBody.CustomerID,
                requestBody.StylistID,
                requestBody.ServiceManagementID,
                requestBody.MessageType,
                requestBody.Status,
                requestBody.IsActive,
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.SearchText,
                requestBody.SortQuery);

            if (result.Status)
                return Ok(_mapper.Map<ListResultObject<BookingScheduledMessageVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("GetBookingScheduledMessageById_Base")]
        public async Task<ActionResult<RowResultObject<BookingScheduledMessageVM>>> GetBookingScheduledMessageById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _bookingScheduledMessageRep.GetBookingScheduledMessageByIdAsync(requestBody.ID);
            if (result.Status)
                return Ok(_mapper.Map<RowResultObject<BookingScheduledMessageVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("ExistBookingScheduledMessage_Base")]
        public async Task<ActionResult<BitResultObject>> ExistBookingScheduledMessage_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _bookingScheduledMessageRep.ExistBookingScheduledMessageAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("AddBookingScheduledMessage_Base")]
        public async Task<ActionResult<BitResultObject>> AddBookingScheduledMessage_Base(AddEditBookingScheduledMessageRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var row = BuildMessage(requestBody, DateTime.Now.ToShamsi());
            var result = await _bookingScheduledMessageRep.AddBookingScheduledMessageAsync(row);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("EditBookingScheduledMessage_Base")]
        public async Task<ActionResult<BitResultObject>> EditBookingScheduledMessage_Base(AddEditBookingScheduledMessageRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var oldRow = await _bookingScheduledMessageRep.GetBookingScheduledMessageByIdAsync(requestBody.ID);
            if (!oldRow.Status || oldRow.Result == null)
                return BadRequest(oldRow);

            var row = BuildMessage(requestBody, oldRow.Result.CreateDate);
            var result = await _bookingScheduledMessageRep.EditBookingScheduledMessageAsync(row);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteBookingScheduledMessage_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteBookingScheduledMessage_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _bookingScheduledMessageRep.RemoveBookingScheduledMessageAsync(requestBody.ID);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("ScheduleBookingMessagesNow_Base")]
        public async Task<ActionResult<BitResultObject>> ScheduleBookingMessagesNow_Base(ScheduleBookingMessagesRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var jobId = BackgroundJob.Enqueue<JobManager>(job => job.ScheduleBookingFollowUpMessages(requestBody.BookingID));
            await AddLogAsync();

            return Ok(new BitResultObject
            {
                Status = true,
                ID = requestBody.BookingID,
                ErrorMessage = $"Job queued: {jobId}"
            });
        }

        [HttpPost("SendBookingScheduledMessageNow_Base")]
        public async Task<ActionResult<BitResultObject>> SendBookingScheduledMessageNow_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var jobId = BackgroundJob.Enqueue<JobManager>(job => job.SendBookingScheduledMessage(requestBody.ID));
            await AddLogAsync();

            return Ok(new BitResultObject
            {
                Status = true,
                ID = requestBody.ID,
                ErrorMessage = $"Job queued: {jobId}"
            });
        }

        private static BookingScheduledMessage BuildMessage(AddEditBookingScheduledMessageRequestBody requestBody, DateTime? createDate)
        {
            return new BookingScheduledMessage
            {
                ID = requestBody.ID,
                CreateDate = createDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingID = requestBody.BookingID,
                StylistID = requestBody.StylistID,
                CustomerID = requestBody.CustomerID,
                ServiceManagementID = requestBody.ServiceManagementID,
                StylistServiceFollowUpSettingID = requestBody.StylistServiceFollowUpSettingID,
                StylistServicePriceVariantID = requestBody.StylistServicePriceVariantID,
                MessageType = requestBody.MessageType,
                MessageText = requestBody.MessageText,
                ScheduledAt = requestBody.ScheduledAt,
                Status = requestBody.Status,
                SentAt = requestBody.SentAt,
                RetryCount = requestBody.RetryCount,
                ProviderMessageID = requestBody.ProviderMessageID,
                ErrorMessage = requestBody.ErrorMessage,
                HangfireJobID = requestBody.HangfireJobID,
                SMSMessageID = requestBody.SMSMessageID,
                NotificationID = requestBody.NotificationID,
                IsActive = requestBody.IsActive,
                Description = requestBody.Description
            };
        }

        private async Task AddLogAsync()
        {
            await _logRep.AddLogAsync(new Log
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LogTime = DateTime.Now.ToShamsi(),
                ActionName = ControllerContext.RouteData.Values["action"]?.ToString()
            });
        }
    }
}
