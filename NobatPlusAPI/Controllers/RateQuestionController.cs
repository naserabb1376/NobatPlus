using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.RateQuestion;
using NobatPlusAPI.Models.Public;
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
    [Route("RateQuestion")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class RateQuestionController : ControllerBase
    {
        IRateQuestionRep _RateQuestionRep;
        ILogRep _logRep;

        public RateQuestionController(IRateQuestionRep RateQuestionRep,ILogRep logRep)
        {
           _RateQuestionRep = RateQuestionRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllRateQuestions_Base")]
        public async Task<ActionResult<ListResultObject<RateQuestion>>> GetAllRateQuestions_Base(GetRateQuestionListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _RateQuestionRep.GetAllRateQuestionsAsync(requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetRateQuestionById_Base")]
        public async Task<ActionResult<RowResultObject<RateQuestion>>> GetRateQuestionById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _RateQuestionRep.GetRateQuestionByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistRateQuestion_Base")]
        public async Task<ActionResult<BitResultObject>> ExistRateQuestion_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _RateQuestionRep.ExistRateQuestionAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddRateQuestion_Base")]
        public async Task<ActionResult<BitResultObject>> AddRateQuestion_Base(AddEditRateQuestionRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            RateQuestion RateQuestion = new RateQuestion()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                RateQuestionText = requestBody.RateQuestionText,
                Description = requestBody.Description,
            };
            var result = await _RateQuestionRep.AddRateQuestionAsync(RateQuestion);
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

        [HttpPut("EditRateQuestion_Base")]
        public async Task<ActionResult<BitResultObject>> EditRateQuestion_Base(AddEditRateQuestionRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _RateQuestionRep.GetRateQuestionByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            RateQuestion RateQuestion = new RateQuestion()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                RateQuestionText = requestBody.RateQuestionText,
                Description = requestBody.Description,
            };
            result = await _RateQuestionRep.EditRateQuestionAsync(RateQuestion);
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

        [HttpDelete("DeleteRateQuestion_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteRateQuestion_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _RateQuestionRep.RemoveRateQuestionAsync(requestBody.ID);
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
