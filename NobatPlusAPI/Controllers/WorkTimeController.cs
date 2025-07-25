using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.WorkTime;
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
    [Route("WorkTime")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class WorkTimeController : ControllerBase
    {
        IWorkTimeRep _WorkTimeRep;
        ILogRep _logRep;

        public WorkTimeController(IWorkTimeRep WorkTimeRep,ILogRep logRep)
        {
           _WorkTimeRep = WorkTimeRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllWorkTimes_Base")]
        public async Task<ActionResult<ListResultObject<WorkTime>>> GetAllWorkTimes_Base(GetWorkTimeListRequestBody requestBody)
        {
            var result = new ListResultObject<WorkTime>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            result = await _WorkTimeRep.GetAllWorkTimesAsync(requestBody.StylistId, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpPost("GetWorkTimeById_Base")]
        public async Task<ActionResult<RowResultObject<WorkTime>>> GetWorkTimeById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _WorkTimeRep.GetWorkTimeByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistWorkTime_Base")]
        public async Task<ActionResult<BitResultObject>> ExistWorkTime_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _WorkTimeRep.ExistWorkTimeAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddWorkTime_Base")]
        public async Task<ActionResult<BitResultObject>> AddWorkTime_Base(AddEditWorkTimeRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            WorkTime WorkTime = new WorkTime()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                WorkStartTime = requestBody.WorkStartTime,
                WorkEndTime = requestBody.WorkEndTime,
                DayOfWeek = requestBody.DayOfWeek,
                StylistID = requestBody.StylistID,
                Description = "",
            };
            var result = await _WorkTimeRep.AddWorkTimeAsync(WorkTime);
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

        [HttpPut("EditWorkTime_Base")]
        public async Task<ActionResult<BitResultObject>> EditWorkTime_Base(AddEditWorkTimeRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theRow = await _WorkTimeRep.GetWorkTimeByIdAsync(requestBody.ID);

            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            WorkTime WorkTime = new WorkTime()
            {
                ID = requestBody.ID,
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                WorkStartTime = requestBody.WorkStartTime,
                WorkEndTime = requestBody.WorkEndTime,
                DayOfWeek = requestBody.DayOfWeek,
                StylistID = requestBody.StylistID,
                Description = "",
            };
             result = await _WorkTimeRep.EditWorkTimeAsync(WorkTime);
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

        [HttpDelete("DeleteWorkTime_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteWorkTime_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _WorkTimeRep.RemoveWorkTimeAsync(requestBody.ID);
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
