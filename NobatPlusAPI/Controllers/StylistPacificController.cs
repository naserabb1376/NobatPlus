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
using NobatPlusAPI.Models.StylistPacific;
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
    [Route("StylistPacific")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class StylistPacificController : ControllerBase
    {
        IStylistPacificRep _StylistPacificRep;
        IBookingRep _BookingRep;
        IStylistRep _stylistRep;
        ICustomerRep _customerRep;
        INotificationRep _notificationRep;
        ISettingRep _SettingRep;
        ISMSMessageRep _sMSMessageRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;

        public StylistPacificController(IStylistPacificRep StylistPacificRep, IBookingRep bookingRep, ILogRep logRep, IMapper mapper, IStylistRep stylistRep, ICustomerRep customerRep, INotificationRep notificationRep, ISMSMessageRep sMSMessageRep, ISettingRep settingRep)
        {
            _StylistPacificRep = StylistPacificRep;
            _BookingRep = bookingRep;
            _logRep = logRep;
            _mapper = mapper;
            _stylistRep = stylistRep;
            _customerRep = customerRep;
            _notificationRep = notificationRep;
            _sMSMessageRep = sMSMessageRep;
            _SettingRep = settingRep;
        }

        [HttpPost("GetAllStylistPacifics_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<StylistPacificVM>>> GetAllStylistPacifics_Base(GetStylistPacificListRequestBody requestBody)
        {
            var result = new ListResultObject<StylistPacific>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            result = await _StylistPacificRep.GetAllStylistPacificsAsync(requestBody.StylistId, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<StylistPacificVM>>(result);
                return Ok(resultVM);
            }

            return BadRequest(result);
        }


        [HttpPost("GetStylistPacificById_Base")]
        public async Task<ActionResult<RowResultObject<StylistPacificVM>>> GetStylistPacificById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistPacificRep.GetStylistPacificByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<StylistPacificVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistStylistPacific_Base")]
        public async Task<ActionResult<BitResultObject>> ExistStylistPacific_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistPacificRep.ExistStylistPacificAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddStylistPacific_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylistPacific_Base(AddEditStylistPacificRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            StylistPacific StylistPacific = new StylistPacific()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PacificStartDate = requestBody.PacificStartDate,
                PacificEndDate = requestBody.PacificEndDate,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description ?? "",
            };
            var result = await _StylistPacificRep.AddStylistPacificAsync(StylistPacific);
            if (result.Status)
            {
                var affectedBookings = await _BookingRep.MarkBookingsForRescheduleByLeaveAsync(
                    requestBody.StylistID,
                    requestBody.PacificStartDate,
                    requestBody.PacificEndDate,
                    requestBody.Description ?? "");

                if (!affectedBookings.Status)
                {
                    await _StylistPacificRep.RemoveStylistPacificAsync(result.ID);
                    return BadRequest(affectedBookings);
                }

                foreach (var booking in affectedBookings.Results)
                {
                    var customer = booking.Customer?.Person;
                    if (customer == null || string.IsNullOrWhiteSpace(customer.PhoneNumber))
                        continue;


                    try
                    {

                        var messageRow = await _SettingRep.GetSettingRowAsync(0, "StylistLeaveMessage");
                        if (messageRow != null && messageRow.Result.IsActive)
                        {
                            var message = messageRow.Result.Value.MakeMessageOnPattern(new List<ToolBox.MessagePatternObj>
                            {
                                new ToolBox.MessagePatternObj
                                {
                                    Variable = "firstname",
                                    Value = customer.FirstName
                                },
                                 new ToolBox.MessagePatternObj
                                {
                                    Variable = "bookingstartdate",
                                    Value = booking.BookingStartDate.ToShamsiString().Split(' ')[0]
                                },
                                 new ToolBox.MessagePatternObj
                                {
                                    Variable = "bookingtime",
                                    Value = $"{booking.BookingStartDate:HH:mm}"
                    }
                            });
                            var sentStatus = await ToolBox.SendSMSMessage(customer.PhoneNumber, message);
                            await _sMSMessageRep.AddSMSMessageAsync(new SMSMessage
                            {
                                CreateDate = DateTime.Now.ToShamsi(),
                                UpdateDate = DateTime.Now.ToShamsi(),
                                PhoneNumber = customer.PhoneNumber,
                                PersonID = customer.ID,
                                Message = message,
                                SentDate = DateTime.Now.ToShamsi(),
                                Description = $"leave-booking:{booking.ID}",
                                SentStatus = sentStatus
                            });

                            await _notificationRep.AddNotificationAsync(new Notification
                            {
                                CreateDate = DateTime.Now.ToShamsi(),
                                UpdateDate = DateTime.Now.ToShamsi(),
                                PersonID = customer.ID,
                                Message = message,
                                SentDate = DateTime.Now.ToShamsi(),
                                Description = $"leave-booking:{booking.ID}"
                            });
                        }
                       
                    }
                    catch
                    {
                        // ثبت مرخصی و تغییر وضعیت نوبت نباید با خطای سرویس پیامک rollback شود.
                    }
                }

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
        private async Task SendPacificMessage(long stylistId,bool beginDuration)
        {
            var stylist = await _stylistRep.GetStylistByIdAsync(stylistId);

            if (stylist.Result == null) return;

            var customers = await _customerRep.GetAllCustomersAsync(stylistId,pageIndex:1,pageSize:0);

            foreach (var customer in customers.Results)
            {
                string message = beginDuration ? $"{customer.Person.FirstName} عزیز\nآرایشگر {stylist.Result.StylistName} از امروز تا پایان مرخصی در دسترس نیست."
    : $"{customer.Person.FirstName} عزیز\nآرایشگر {stylist.Result.StylistName} از امروز دوباره فعال است.";


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

        // 📩 ارسال پیام پایان مرخصی
        
        [HttpPut("EditStylistPacific_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylistPacific_Base(AddEditStylistPacificRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var theRow = await _StylistPacificRep.GetStylistPacificByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
                return BadRequest(result);
            }

            // بازه قبلی را نگه می‌داریم تا بعد از ویرایش نوبت‌های خارج‌شده را برگردانیم
            var oldStart = theRow.Result.PacificStartDate;
            var oldEnd = theRow.Result.PacificEndDate;

            StylistPacific StylistPacific = new StylistPacific()
            {
                ID = requestBody.ID,
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                PacificStartDate = requestBody.PacificStartDate,
                PacificEndDate = requestBody.PacificEndDate,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description ?? "",
            };
            result = await _StylistPacificRep.EditStylistPacificAsync(StylistPacific);
            if (!result.Status)
                return BadRequest(result);

            // نوبت‌هایی که در بازه قدیمی بودند ولی در بازه جدید نیستند باید برگردند به 1
            var restoredBookings = await _BookingRep.RestoreBookingsAfterLeaveDeleteAsync(
                requestBody.StylistID, oldStart, oldEnd);

            // نوبت‌های جدیداً متاثر شده (بازه جدید)
            var affectedBookings = await _BookingRep.MarkBookingsForRescheduleByLeaveAsync(
                requestBody.StylistID,
                requestBody.PacificStartDate,
                requestBody.PacificEndDate,
                requestBody.Description ?? "");

            if (!affectedBookings.Status)
                return BadRequest(affectedBookings);

            // ارسال SMS/Notification به مشتریان نوبت‌های تازه متاثر شده
            foreach (var booking in affectedBookings.Results)
            {
                var customer = booking.Customer?.Person;
                if (customer == null || string.IsNullOrWhiteSpace(customer.PhoneNumber))
                    continue;

                var message =
                    $"{customer.FirstName} عزیز، نوبت شما در تاریخ " +
                    $"{booking.BookingStartDate.ToShamsiString().Split(' ')[0]} ساعت " +
                    $"{booking.BookingStartDate:HH:mm} به دلیل تغییر برنامه آرایشگر نیازمند تعیین تکلیف است. " +
                    "لطفا از پنل نوبتیکس زمان جدید انتخاب کنید یا نوبت را لغو کنید.";
                try
                {
                    var messageRow = await _SettingRep.GetSettingRowAsync(0, "StylistLeaveMessage");
                    if (messageRow != null && messageRow.Result.IsActive)
                    {
                        var messagex = messageRow.Result.Value.MakeMessageOnPattern(new List<ToolBox.MessagePatternObj>
        {
            new ToolBox.MessagePatternObj
            {
                Variable = "firstname",
                Value = customer.FirstName
            },
             new ToolBox.MessagePatternObj
            {
                Variable = "bookingstartdate",
                Value = booking.BookingStartDate.ToShamsiString().Split(' ')[0]
            },
             new ToolBox.MessagePatternObj
            {
                Variable = "bookingtime",
                Value = $"{booking.BookingStartDate:HH:mm}"
}
        });
                        var sentStatus = await ToolBox.SendSMSMessage(customer.PhoneNumber, messagex);
                        await _sMSMessageRep.AddSMSMessageAsync(new SMSMessage
                        {
                            CreateDate = DateTime.Now.ToShamsi(),
                            UpdateDate = DateTime.Now.ToShamsi(),
                            PhoneNumber = customer.PhoneNumber,
                            PersonID = customer.ID,
                            Message = message,
                            SentDate = DateTime.Now.ToShamsi(),
                            Description = $"leave-edit-booking:{booking.ID}",
                            SentStatus = sentStatus
                        });
                        await _notificationRep.AddNotificationAsync(new Notification
                        {
                            CreateDate = DateTime.Now.ToShamsi(),
                            UpdateDate = DateTime.Now.ToShamsi(),
                            PersonID = customer.ID,
                            Message = message,
                            SentDate = DateTime.Now.ToShamsi(),
                            Description = $"leave-edit-booking:{booking.ID}"
                        });
                    }
                    }
                catch { }
            }

            // اطلاع‌رسانی به مشتریانی که نوبتشان بازگردانده شد
            foreach (var booking in restoredBookings.Results)
            {
                var customer = booking.Customer?.Person;
                if (customer == null || string.IsNullOrWhiteSpace(customer.PhoneNumber))
                    continue;

                 var message =
                    $"{customer.FirstName} عزیز، نوبت شما در تاریخ " +
                    $"{booking.BookingStartDate.ToShamsiString().Split(' ')[0]} ساعت " +
                    $"{booking.BookingStartDate:HH:mm} مجدداً فعال شد. آرایشگر در این زمان در دسترس است.";
                try
                {
                    var sentStatus = await ToolBox.SendSMSMessage(customer.PhoneNumber, message);
                    await _sMSMessageRep.AddSMSMessageAsync(new SMSMessage
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        PhoneNumber = customer.PhoneNumber,
                        PersonID = customer.ID,
                        Message = message,
                        SentDate = DateTime.Now.ToShamsi(),
                        Description = $"leave-restore-booking:{booking.ID}",
                        SentStatus = sentStatus
                    });
                    await _notificationRep.AddNotificationAsync(new Notification
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        PersonID = customer.ID,
                        Message = message,
                        SentDate = DateTime.Now.ToShamsi(),
                        Description = $"leave-restore-booking:{booking.ID}"
                    });
                }
                catch { }
            }

            await _logRep.AddLogAsync(new Log
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LogTime = DateTime.Now.ToShamsi(),
                ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
            });

            return Ok(result);
        }

        [HttpDelete("DeleteStylistPacific_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistPacific_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            // قبل از حذف، بازه مرخصی را می‌گیریم تا بتوانیم نوبت‌ها را برگردانیم
            var theRow = await _StylistPacificRep.GetStylistPacificByIdAsync(requestBody.ID);
            if (!theRow.Status)
                return BadRequest(new BitResultObject { Status = false, ErrorMessage = "مرخصی یافت نشد" });

            var leaveStart = theRow.Result.PacificStartDate;
            var leaveEnd = theRow.Result.PacificEndDate;
            var stylistId = theRow.Result.StylistID;

            var result = await _StylistPacificRep.RemoveStylistPacificAsync(requestBody.ID);
            if (!result.Status)
                return BadRequest(result);

            // نوبت‌های Status==5 که دیگر توسط هیچ مرخصی دیگری پوشش نمی‌شوند برمی‌گردند به Status==1
            var restoredBookings = await _BookingRep.RestoreBookingsAfterLeaveDeleteAsync(
                stylistId, leaveStart, leaveEnd);

            // اطلاع‌رسانی به مشتریان که نوبتشان مجدداً فعال شد
            foreach (var booking in restoredBookings.Results)
            {
                var customer = booking.Customer?.Person;
                if (customer == null || string.IsNullOrWhiteSpace(customer.PhoneNumber))
                    continue;

                var message =
                    $"{customer.FirstName} عزیز، نوبت شما در تاریخ " +
                    $"{booking.BookingStartDate.ToShamsiString().Split(' ')[0]} ساعت " +
                    $"{booking.BookingStartDate:HH:mm} مجدداً فعال شد. آرایشگر در این زمان در دسترس است.";
                try
                {
                    var sentStatus = await ToolBox.SendSMSMessage(customer.PhoneNumber, message);
                    await _sMSMessageRep.AddSMSMessageAsync(new SMSMessage
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        PhoneNumber = customer.PhoneNumber,
                        PersonID = customer.ID,
                        Message = message,
                        SentDate = DateTime.Now.ToShamsi(),
                        Description = $"leave-restore-booking:{booking.ID}",
                        SentStatus = sentStatus
                    });
                    await _notificationRep.AddNotificationAsync(new Notification
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        PersonID = customer.ID,
                        Message = message,
                        SentDate = DateTime.Now.ToShamsi(),
                        Description = $"leave-restore-booking:{booking.ID}"
                    });
                }
                catch { }
            }

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
