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
        IStylistRep _stylistRep;
        ICustomerRep _customerRep;
        INotificationRep _notificationRep;
        ISMSMessageRep _sMSMessageRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;

        public StylistPacificController(IStylistPacificRep StylistPacificRep,ILogRep logRep, IMapper mapper, IStylistRep stylistRep, ICustomerRep customerRep, INotificationRep notificationRep, ISMSMessageRep sMSMessageRep)
        {
            _StylistPacificRep = StylistPacificRep;
            _logRep = logRep;
            _mapper = mapper;
            _stylistRep = stylistRep;
            _customerRep = customerRep;
            _notificationRep = notificationRep;
            _sMSMessageRep = sMSMessageRep;
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
                Description = "",
            };
            var result = await _StylistPacificRep.AddStylistPacificAsync(StylistPacific);
            if (result.Status)
            {
                // 🟡 زمان‌بندی ارسال پیام شروع مرخصی
                 BackgroundJob.Schedule(() => SendPacificMessage(requestBody.StylistID,true), requestBody.PacificStartDate);

                // 🟢 زمان‌بندی ارسال پیام پایان مرخصی
                BackgroundJob.Schedule(() => SendPacificMessage(requestBody.StylistID,false), requestBody.PacificEndDate);

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
            {
                return BadRequest(requestBody);
            }

            var theRow = await _StylistPacificRep.GetStylistPacificByIdAsync(requestBody.ID);

            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            StylistPacific StylistPacific = new StylistPacific()
            {
                ID = requestBody.ID,
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                PacificStartDate = requestBody.PacificStartDate,
                PacificEndDate = requestBody.PacificEndDate,
                StylistID = requestBody.StylistID,
                Description = "",
            };
             result = await _StylistPacificRep.EditStylistPacificAsync(StylistPacific);
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

        [HttpDelete("DeleteStylistPacific_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistPacific_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistPacificRep.RemoveStylistPacificAsync(requestBody.ID);
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
