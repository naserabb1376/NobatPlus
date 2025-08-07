using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.SMSMessage;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NobatPlusAPI.Controllers
{
    [Route("SMSMessage")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class SMSMessageController : ControllerBase
    {
        ISMSMessageRep _SMSMessageRep;
        ILoginRep _LoginRep;
        ILogRep _logRep;

        public SMSMessageController(ISMSMessageRep SMSMessageRep,ILoginRep loginRep,ILogRep logRep)
        {
           _SMSMessageRep = SMSMessageRep;
            _LoginRep = loginRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllSMSMessages_Base")]
        public async Task<ActionResult<ListResultObject<SMSMessage>>> GetAllSMSMessages_Base(GetSMSMessageListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SMSMessageRep.GetAllSMSMessagesAsync(requestBody.PersonId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetSMSMessageById_Base")]
        public async Task<ActionResult<RowResultObject<SMSMessage>>> GetSMSMessageById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SMSMessageRep.GetSMSMessageByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistSMSMessage_Base")]
        public async Task<ActionResult<BitResultObject>> ExistSMSMessage_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SMSMessageRep.ExistSMSMessageAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddSMSMessage_Base")]
        public async Task<ActionResult<BitResultObject>> AddSMSMessage_Base(AddEditSMSMessageRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var validPhoneNumber = await _LoginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");

            if (requestBody.PhoneNumberExists)
            {
                if (!validPhoneNumber.Status && string.IsNullOrEmpty(validPhoneNumber.ErrorMessage))
                {
                    result.Status = validPhoneNumber.Status;
                    result.ErrorMessage = "این کاربر در سیستم وجود ندارد";
                    return BadRequest(result);
                }
            }

            else
            {
                if (validPhoneNumber.Status)
                {
                    result.Status = !validPhoneNumber.Status;
                    result.ErrorMessage = "شماره تماس تکراری است";
                    return BadRequest(result);
                }
            }

            bool sentstatus = await ToolBox.SendSMSMessage(requestBody.PhoneNumber,requestBody.Message);

          

            SMSMessage SMSMessage = new SMSMessage()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PhoneNumber = requestBody.PhoneNumber,
                PersonID = validPhoneNumber.ID,
                Message = requestBody.Message,
                SentDate = string.IsNullOrEmpty(requestBody.SentDate) ? DateTime.Now.ToShamsi() : requestBody.SentDate.StringToDate(),
                Description = requestBody.Description,
                SentStatus = sentstatus,
            };
             result = await _SMSMessageRep.AddSMSMessageAsync(SMSMessage);
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


                if (sentstatus)
                {
                    result.ErrorMessage = $"پیامک ارسال شد";
                    return Ok(result);
                }
                else
                {
                    result.ErrorMessage = $"در ارسال پیامک مشکلی بوجود آمد";
                }
            }
            return BadRequest(result);
        }

        [HttpPut("EditSMSMessage_Base")]
        public async Task<ActionResult<BitResultObject>> EditSMSMessage_Base(AddEditSMSMessageRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var validPhoneNumber = await _LoginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");

            var theRow = await _SMSMessageRep.GetSMSMessageByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            SMSMessage SMSMessage = new SMSMessage()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                PhoneNumber = requestBody.PhoneNumber,
                PersonID = validPhoneNumber.ID,
                Message = requestBody.Message,
                SentDate = string.IsNullOrEmpty(requestBody.SentDate) ? DateTime.Now.ToShamsi() : requestBody.SentDate.StringToDate(),
                Description = requestBody.Description,
            };
            result = await _SMSMessageRep.EditSMSMessageAsync(SMSMessage);
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

        [HttpDelete("DeleteSMSMessage_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteSMSMessage_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _SMSMessageRep.RemoveSMSMessageAsync(requestBody.ID);
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
