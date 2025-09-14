using AutoMapper;
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
using NobatPlusAPI.Models.RateHistory;
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
    [Route("RateHistory")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class RateHistoryController : ControllerBase
    {
        IRateHistoryRep _RateHistoryRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;

        public RateHistoryController(IRateHistoryRep RateHistoryRep,ILogRep logRep,IMapper mapper)
        {
           _RateHistoryRep = RateHistoryRep;
           _logRep = logRep;
           _mapper = mapper;
        }

        [HttpPost("GetAllRateHistories_Base")]
        public async Task<ActionResult<ListResultObject<RateHistoryVM>>> GetAllRateHistories_Base(GetRateHistoryListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _RateHistoryRep.GetAllRateHistoriesAsync(requestBody.CustomerId,requestBody.StylistId,requestBody.RateQuestionId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<RateHistoryVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetRateHistoryById_Base")]
        public async Task<ActionResult<RowResultObject<RateHistoryVM>>> GetRateHistoryById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _RateHistoryRep.GetRateHistoryByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<RateHistoryVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistRateHistory_Base")]
        public async Task<ActionResult<BitResultObject>> ExistRateHistory_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _RateHistoryRep.ExistRateHistoryAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddRateHistory_Base")]
        public async Task<ActionResult<BitResultObject>> AddRateHistory_Base(AddEditRateHistoryRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            RateHistory RateHistory = new RateHistory()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                RateDate = string.IsNullOrEmpty(requestBody.RateDate) ? DateTime.Now.ToShamsi() : requestBody.RateDate.StringToDate(),
                RateQuestionID = requestBody.RateQuestionID,
                StylistID = requestBody.StylistID,
                CustomerID = requestBody.CustomerID,
                RateScore = requestBody.RateScore,
                Description = requestBody.Description,
            };
            var result = await _RateHistoryRep.AddRateHistoryAsync(RateHistory);
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

        [HttpPut("EditRateHistory_Base")]
        public async Task<ActionResult<BitResultObject>> EditRateHistory_Base(AddEditRateHistoryRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _RateHistoryRep.GetRateHistoryByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            RateHistory RateHistory = new RateHistory()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                RateDate = string.IsNullOrEmpty(requestBody.RateDate) ? DateTime.Now.ToShamsi() : requestBody.RateDate.StringToDate(),
                RateQuestionID = requestBody.RateQuestionID,
                StylistID = requestBody.StylistID,
                CustomerID = requestBody.CustomerID,
                RateScore = requestBody.RateScore,
                Description = requestBody.Description,
            };
            result = await _RateHistoryRep.EditRateHistoryAsync(RateHistory);
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

        [HttpDelete("DeleteRateHistory_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteRateHistory_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _RateHistoryRep.RemoveRateHistoryAsync(requestBody.ID);
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
