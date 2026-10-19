using AutoMapper;
using Domain;
using Domains;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.WorkerPacific;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.ServiceModel.Channels;
using System.Text;

namespace NobatPlusAPI.Controllers
{
    [Route("WorkerPacific")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class WorkerPacificController : ControllerBase
    {
        IWorkerPacificRep _WorkerPacificRep;
        IBookingRep _BookingRep;
        IWorkerRep _workerRep;
        ICustomerRep _customerRep;
        INotificationRep _notificationRep;
        ISettingRep _SettingRep;
        ISMSMessageRep _sMSMessageRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;

        public WorkerPacificController(IWorkerPacificRep WorkerPacificRep, IBookingRep bookingRep, ILogRep logRep, IMapper mapper, IWorkerRep workerRep, ICustomerRep customerRep, INotificationRep notificationRep, ISMSMessageRep sMSMessageRep, ISettingRep settingRep)
        {
            _WorkerPacificRep = WorkerPacificRep;
            _BookingRep = bookingRep;
            _logRep = logRep;
            _mapper = mapper;
            _workerRep = workerRep;
            _customerRep = customerRep;
            _notificationRep = notificationRep;
            _sMSMessageRep = sMSMessageRep;
            _SettingRep = settingRep;
        }

        [HttpPost("GetAllWorkerPacifics_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<WorkerPacificVM>>> GetAllWorkerPacifics_Base(GetWorkerPacificListRequestBody requestBody)
        {
            var result = new ListResultObject<WorkerPacific>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            result = await _WorkerPacificRep.GetAllWorkerPacificsAsync(requestBody.WorkerId, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<WorkerPacificVM>>(result);
                return Ok(resultVM);
            }

            return BadRequest(result);
        }


        [HttpPost("GetWorkerPacificById_Base")]
        public async Task<ActionResult<RowResultObject<WorkerPacificVM>>> GetWorkerPacificById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _WorkerPacificRep.GetWorkerPacificByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<WorkerPacificVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistWorkerPacific_Base")]
        public async Task<ActionResult<BitResultObject>> ExistWorkerPacific_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _WorkerPacificRep.ExistWorkerPacificAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddWorkerPacific_Base")]
        public async Task<ActionResult<BitResultObject>> AddWorkerPacific_Base(AddEditWorkerPacificRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            WorkerPacific WorkerPacific = new WorkerPacific()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PacificStartDate = requestBody.PacificStartDate,
                PacificEndDate = requestBody.PacificEndDate,
                WorkerID = requestBody.WorkerID,
                Description = requestBody.Description ?? "",
            };
            var result = await _WorkerPacificRep.AddWorkerPacificAsync(WorkerPacific);
            if (result.Status)
            {
                var affectedBookings = await _BookingRep.MarkBookingsForRescheduleByLeaveAsync(
                    requestBody.WorkerID,
                    requestBody.PacificStartDate,
                    requestBody.PacificEndDate,
                    requestBody.Description ?? "");

                if (!affectedBookings.Status)
                {
                    await _WorkerPacificRep.RemoveWorkerPacificAsync(result.ID);
                    return BadRequest(affectedBookings);
                }

                await SendAffectedBookingLeaveMessagesAsync(
                    affectedBookings.Results,
                    result.ID,
                    requestBody.PacificStartDate,
                    requestBody.PacificEndDate,
                    requestBody.Description ?? "");

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

        // 📩 ارسال پیام آغاز مرخصی
        private async Task SendPacificMessage(long workerId,bool beginDuration)
        {
            var worker = await _workerRep.GetWorkerByIdAsync(workerId);

            if (worker.Result == null) return;

            var customers = await _customerRep.GetAllCustomersAsync(workerId,pageIndex:1,pageSize:0);

            foreach (var customer in customers.Results)
            {
                string message = beginDuration ? $"{customer.Person.FirstName} عزیز\nخدمات دهنده {worker.Result.WorkerName} از امروز تا پایان مرخصی در دسترس نیست."
    : $"{customer.Person.FirstName} عزیز\nخدمات دهنده {worker.Result.WorkerName} از امروز دوباره فعال است.";


                #region SendSMS

                bool sentstatus = await ToolBox.SendSMSMessage(customer.Person.PhoneNumber, message);



                SMSMessage SMSMessage = new SMSMessage()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    PhoneNumber = customer.Person.PhoneNumber,
                    PersonID = customer.PersonID,
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
                    PersonID = customer.PersonID,
                    Message = message,
                    SentDate = DateTime.Now.ToShamsi() ,
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
        }

        private async Task SendAffectedBookingLeaveMessagesAsync(
            IEnumerable<BookingDTO> bookings,
            long leaveId,
            DateTime leaveStart,
            DateTime leaveEnd,
            string leaveReason)
        {
            var affectedBookings = bookings?.ToList() ?? new List<BookingDTO>();
            if (affectedBookings.Count == 0)
                return;

            var messageRow = await _SettingRep.GetSettingRowAsync(0, "WorkerLeaveMessage");
            if (!messageRow.Status ||
                messageRow.Result == null ||
                !messageRow.Result.IsActive ||
                string.IsNullOrWhiteSpace(messageRow.Result.Value))
                return;

            foreach (var booking in affectedBookings)
            {
                var customer = booking.Customer?.Person;
                if (customer == null)
                    continue;

                var messageKey =
                    $"leave-booking:{leaveId}:{booking.ID}:{leaveStart.Ticks}:{leaveEnd.Ticks}";

                if (await _sMSMessageRep.HasMessageWithDescriptionAsync(messageKey))
                    continue;

                var bookingStart = booking.BookingStartDate.ToShamsiString().Split(' ');
                var bookingEnd = booking.BookingEndDate.ToShamsiString().Split(' ');
                var leaveStartParts = leaveStart.ToShamsiString().Split(' ');
                var leaveEndParts = leaveEnd.ToShamsiString().Split(' ');
                var workerName = booking.Worker?.WorkerName ?? "";

                var message = messageRow.Result.Value.MakeMessageOnPattern(new List<ToolBox.MessagePatternObj>
                {
                    new() { Variable = "firstname", Value = customer.FirstName ?? "" },
                    new() { Variable = "customerfullname", Value = $"{customer.FirstName} {customer.LastName}".Trim() },
                    new() { Variable = "workername", Value = workerName },
                    new() { Variable = "bookingdate", Value = bookingStart[0] },
                    new() { Variable = "bookingstartdate", Value = bookingStart[0] },
                    new() { Variable = "bookingtime", Value = bookingStart.Length > 1 ? bookingStart[1] : booking.BookingStartDate.ToString("HH:mm") },
                    new() { Variable = "bookingenddate", Value = bookingEnd[0] },
                    new() { Variable = "bookingendtime", Value = bookingEnd.Length > 1 ? bookingEnd[1] : booking.BookingEndDate.ToString("HH:mm") },
                    new() { Variable = "leavestartdate", Value = leaveStartParts[0] },
                    new() { Variable = "leavestarttime", Value = leaveStartParts.Length > 1 ? leaveStartParts[1] : leaveStart.ToString("HH:mm") },
                    new() { Variable = "leaveenddate", Value = leaveEndParts[0] },
                    new() { Variable = "leaveendtime", Value = leaveEndParts.Length > 1 ? leaveEndParts[1] : leaveEnd.ToString("HH:mm") },
                    new() { Variable = "leavereason", Value = leaveReason }
                });

                try
                {
                    var now = DateTime.Now.ToShamsi();
                    var sentStatus = !string.IsNullOrWhiteSpace(customer.PhoneNumber) &&
                                     await ToolBox.SendSMSMessage(customer.PhoneNumber, message);

                    await _sMSMessageRep.AddSMSMessageAsync(new SMSMessage
                    {
                        CreateDate = now,
                        UpdateDate = now,
                        PhoneNumber = customer.PhoneNumber ?? "",
                        PersonID = booking.Customer.PersonID,
                        Message = message,
                        SentDate = now,
                        Description = messageKey,
                        SentStatus = sentStatus
                    });

                    await _notificationRep.AddNotificationAsync(new Notification
                    {
                        CreateDate = now,
                        UpdateDate = now,
                        PersonID = booking.Customer.PersonID,
                        Message = message,
                        SentDate = now,
                        Description = messageKey
                    });
                }
                catch (Exception ex)
                {
                    ToolBox.SaveLog(ex.Message + '\n' + ex.InnerException?.Message);
                }
            }
        }

        private async Task SendRestoredBookingMessagesAsync(
            IEnumerable<BookingDTO> bookings,
            long leaveId,
            DateTime leaveStart,
            DateTime leaveEnd)
        {
            var restoredBookings = bookings?.ToList() ?? new List<BookingDTO>();
            if (restoredBookings.Count == 0)
                return;

            var messageRow = await _SettingRep.GetSettingRowAsync(0, "WorkerLeaveRestoreMessage");
            if (!messageRow.Status ||
                messageRow.Result == null ||
                !messageRow.Result.IsActive ||
                string.IsNullOrWhiteSpace(messageRow.Result.Value))
                return;

            foreach (var booking in restoredBookings)
            {
                var customer = booking.Customer?.Person;
                if (customer == null)
                    continue;

                var messageKey =
                    $"leave-restore-booking:{leaveId}:{booking.ID}:{leaveStart.Ticks}:{leaveEnd.Ticks}";

                if (await _sMSMessageRep.HasMessageWithDescriptionAsync(messageKey))
                    continue;

                var bookingStart = booking.BookingStartDate.ToShamsiString().Split(' ');
                var bookingEnd = booking.BookingEndDate.ToShamsiString().Split(' ');
                var leaveStartParts = leaveStart.ToShamsiString().Split(' ');
                var leaveEndParts = leaveEnd.ToShamsiString().Split(' ');

                var message = messageRow.Result.Value.MakeMessageOnPattern(new List<ToolBox.MessagePatternObj>
                {
                    new() { Variable = "firstname", Value = customer.FirstName ?? "" },
                    new() { Variable = "customerfullname", Value = $"{customer.FirstName} {customer.LastName}".Trim() },
                    new() { Variable = "workername", Value = booking.Worker?.WorkerName ?? "" },
                    new() { Variable = "bookingdate", Value = bookingStart[0] },
                    new() { Variable = "bookingstartdate", Value = bookingStart[0] },
                    new() { Variable = "bookingtime", Value = bookingStart.Length > 1 ? bookingStart[1] : booking.BookingStartDate.ToString("HH:mm") },
                    new() { Variable = "bookingenddate", Value = bookingEnd[0] },
                    new() { Variable = "bookingendtime", Value = bookingEnd.Length > 1 ? bookingEnd[1] : booking.BookingEndDate.ToString("HH:mm") },
                    new() { Variable = "leavestartdate", Value = leaveStartParts[0] },
                    new() { Variable = "leavestarttime", Value = leaveStartParts.Length > 1 ? leaveStartParts[1] : leaveStart.ToString("HH:mm") },
                    new() { Variable = "leaveenddate", Value = leaveEndParts[0] },
                    new() { Variable = "leaveendtime", Value = leaveEndParts.Length > 1 ? leaveEndParts[1] : leaveEnd.ToString("HH:mm") }
                });

                try
                {
                    var now = DateTime.Now.ToShamsi();
                    var sentStatus = !string.IsNullOrWhiteSpace(customer.PhoneNumber) &&
                                     await ToolBox.SendSMSMessage(customer.PhoneNumber, message);

                    await _sMSMessageRep.AddSMSMessageAsync(new SMSMessage
                    {
                        CreateDate = now,
                        UpdateDate = now,
                        PhoneNumber = customer.PhoneNumber ?? "",
                        PersonID = booking.Customer.PersonID,
                        Message = message,
                        SentDate = now,
                        Description = messageKey,
                        SentStatus = sentStatus
                    });

                    await _notificationRep.AddNotificationAsync(new Notification
                    {
                        CreateDate = now,
                        UpdateDate = now,
                        PersonID = booking.Customer.PersonID,
                        Message = message,
                        SentDate = now,
                        Description = messageKey
                    });
                }
                catch (Exception ex)
                {
                    ToolBox.SaveLog(ex.Message + '\n' + ex.InnerException?.Message);
                }
            }
        }

        // 📩 ارسال پیام پایان مرخصی
        
        [HttpPut("EditWorkerPacific_Base")]
        public async Task<ActionResult<BitResultObject>> EditWorkerPacific_Base(AddEditWorkerPacificRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var theRow = await _WorkerPacificRep.GetWorkerPacificByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
                return BadRequest(result);
            }

            // بازه قبلی را نگه می‌داریم تا بعد از ویرایش نوبت‌های خارج‌شده را برگردانیم
            var oldStart = theRow.Result.PacificStartDate;
            var oldEnd = theRow.Result.PacificEndDate;
            var oldWorkerId = theRow.Result.WorkerID;

            WorkerPacific WorkerPacific = new WorkerPacific()
            {
                ID = requestBody.ID,
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                PacificStartDate = requestBody.PacificStartDate,
                PacificEndDate = requestBody.PacificEndDate,
                WorkerID = requestBody.WorkerID,
                Description = requestBody.Description ?? "",
            };
            result = await _WorkerPacificRep.EditWorkerPacificAsync(WorkerPacific);
            if (!result.Status)
                return BadRequest(result);

            // نوبت‌هایی که در بازه قدیمی بودند ولی در بازه جدید نیستند باید برگردند به 1
            var restoredBookings = await _BookingRep.RestoreBookingsAfterLeaveDeleteAsync(
                oldWorkerId, oldStart, oldEnd);

            // نوبت‌های جدیداً متاثر شده (بازه جدید)
            var affectedBookings = await _BookingRep.MarkBookingsForRescheduleByLeaveAsync(
                requestBody.WorkerID,
                requestBody.PacificStartDate,
                requestBody.PacificEndDate,
                requestBody.Description ?? "");

            if (!affectedBookings.Status)
                return BadRequest(affectedBookings);

            await SendAffectedBookingLeaveMessagesAsync(
                affectedBookings.Results,
                requestBody.ID,
                requestBody.PacificStartDate,
                requestBody.PacificEndDate,
                requestBody.Description ?? "");

            await SendRestoredBookingMessagesAsync(
                restoredBookings.Results,
                requestBody.ID,
                oldStart,
                oldEnd);

            await _logRep.AddLogAsync(new Log
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LogTime = DateTime.Now.ToShamsi(),
                ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
            });

            return Ok(result);
        }

        [HttpDelete("DeleteWorkerPacific_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteWorkerPacific_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            // قبل از حذف، بازه مرخصی را می‌گیریم تا بتوانیم نوبت‌ها را برگردانیم
            var theRow = await _WorkerPacificRep.GetWorkerPacificByIdAsync(requestBody.ID);
            if (!theRow.Status)
                return BadRequest(new BitResultObject { Status = false, ErrorMessage = "مرخصی یافت نشد" });

            var leaveStart = theRow.Result.PacificStartDate;
            var leaveEnd = theRow.Result.PacificEndDate;
            var workerId = theRow.Result.WorkerID;

            var result = await _WorkerPacificRep.RemoveWorkerPacificAsync(requestBody.ID);
            if (!result.Status)
                return BadRequest(result);

            // نوبت‌های Status==5 که دیگر توسط هیچ مرخصی دیگری پوشش نمی‌شوند برمی‌گردند به Status==1
            var restoredBookings = await _BookingRep.RestoreBookingsAfterLeaveDeleteAsync(
                workerId, leaveStart, leaveEnd);

            await SendRestoredBookingMessagesAsync(
                restoredBookings.Results,
                requestBody.ID,
                leaveStart,
                leaveEnd);

            await _logRep.AddLogAsync(new Log
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LogTime = DateTime.Now.ToShamsi(),
                ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
            });

            return Ok(result);
        }
    }
}
