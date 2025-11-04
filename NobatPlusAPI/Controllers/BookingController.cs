using AutoMapper;
using Domain;
using Domains;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Booking;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NobatPlusAPI.Controllers
{
    [Route("Booking")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class BookingController : ControllerBase
    {
        IBookingRep _BookingRep;
        INotificationRep _notificationRep;
        ISMSMessageRep _sMSMessageRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public BookingController(IBookingRep BookingRep,ILogRep logRep, IMapper mapper, INotificationRep notificationRep, ISMSMessageRep sMSMessageRep)
        {
           _BookingRep = BookingRep;
            _logRep = logRep;
            _mapper = mapper;
            _notificationRep = notificationRep;
            _sMSMessageRep = sMSMessageRep;
        }

        [HttpPost("GetAllBookings_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<BookingVM>>> GetAllBookings_Base(GetBookingListRequestBody requestBody)
        {
            var result = new ListResultObject<BookingDTO>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            result = await _BookingRep.GetAllBookingsAsync(requestBody.ServiceId,requestBody.CancelState, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<BookingVM>>(result);
                return Ok(resultVM);
            }

            return BadRequest(result);
        }


        [HttpPost("GetBookingById_Base")]
        public async Task<ActionResult<RowResultObject<BookingVM>>> GetBookingById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingRep.GetBookingByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<BookingVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistBooking_Base")]
        public async Task<ActionResult<BitResultObject>> ExistBooking_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingRep.ExistBookingAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddBooking_Base")]
        public async Task<ActionResult<BitResultObject>> AddBooking_Base(AddEditBookingRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Booking Booking = new Booking()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingDate = requestBody.BookingDate,
                BookingTime = requestBody.BookingTime,
                CancelReason = requestBody.CancelReason ?? "",
                CustomerID = requestBody.CustomerID,
                IsCancelled = requestBody.IsCancelled,
                Status = requestBody.Status,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description,
            };
            var result = await _BookingRep.AddBookingAsync(Booking);
            if (result.Status)
            {
                BackgroundJob.Schedule(() => SendBookingRemindMessage(result.ID), requestBody.BookingDate.AddDays(-1));


                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
               await _logRep.AddLogAsync(log);

                #endregion


                return Ok(result);
            }
            return BadRequest(result);
        }

        private async Task SendBookingRemindMessage(long bookingId)
        {
            var booking = await _BookingRep.GetBookingByIdAsync(bookingId);

            if (booking.Result == null) return;


            string message = $@"
{booking.Result.Customer.Person.FirstName} عزیز
یادت نره که فردا {booking.Result.BookingDate.ToShamsiString()}
پیش {booking.Result.Stylist.Person.FirstName} {booking.Result.Stylist.Person.LastName} نوبت داری
میبینیمت!
";


            #region SendSMS

            bool sentstatus = await ToolBox.SendSMSMessage(booking.Result.Customer.Person.PhoneNumber, message);



            SMSMessage SMSMessage = new SMSMessage()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PhoneNumber = booking.Result.Customer.Person.PhoneNumber,
                PersonID = booking.Result.Customer.PersonID,
                Message = message,
                SentDate = DateTime.Now.ToShamsi(),
                Description = message,
                SentStatus = sentstatus,
            };
            var smsresult = await _sMSMessageRep.AddSMSMessageAsync(SMSMessage);
            if (smsresult.Status)
            {
                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion
            }

            #endregion

            #region SendNotification

            Notification Notification = new Notification()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PersonID = booking.Result.Customer.PersonID,
                Message = message,
                SentDate = DateTime.Now.ToShamsi(),
                Description = message,
            };
            var notifresult = await _notificationRep.AddNotificationAsync(Notification);
            if (notifresult.Status)
            {
                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion
            }

            #endregion
        }


        [HttpPut("EditBooking_Base")]
        public async Task<ActionResult<BitResultObject>> EditBooking_Base(AddEditBookingRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theRow = await _BookingRep.GetBookingByIdAsync(requestBody.ID);

            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Booking Booking = new Booking()
            {
                ID = requestBody.ID,
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingDate = requestBody.BookingDate,
                BookingTime = requestBody.BookingTime,
                CancelReason = requestBody.CancelReason ?? "",
                CustomerID = requestBody.CustomerID,
                IsCancelled = requestBody.IsCancelled,
                Status = requestBody.Status,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description,
            };
             result = await _BookingRep.EditBookingAsync(Booking);
            if (result.Status)
            {

                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion

                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("DeleteBooking_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteBooking_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingRep.RemoveBookingAsync(requestBody.ID);
            if (result.Status)
            {

                #region AddLog

                Log log = new Log()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                };
                await _logRep.AddLogAsync(log);

                #endregion

                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
