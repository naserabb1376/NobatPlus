using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.JobType;
using NobatPlusAPI.Models.Log;
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
    [Route("Log")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class LogController : ControllerBase
    {
        ILogRep _LogRep;

        public LogController(ILogRep LogRep)
        {
           _LogRep = LogRep;
        }

        [HttpPost("GetAllLogs_Base")]
        public async Task<ActionResult<ListResultObject<Log>>> GetAllLogs_Base(GetLogListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LogRep.GetAllLogsAsync(requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetLogById_Base")]
        public async Task<ActionResult<RowResultObject<Log>>> GetLogById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LogRep.GetLogByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistLog_Base")]
        public async Task<ActionResult<BitResultObject>> ExistLog_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LogRep.ExistLogAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddLog_Base")]
        public async Task<ActionResult<BitResultObject>> AddLog_Base(AddEditLogRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Log Log = new Log()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                ActionName = requestBody.ActionName,
                LogTime= requestBody.LogTime,
                Description = requestBody.Description,
            };
            var result = await _LogRep.AddLogAsync(Log);
            if (result.Status)
            {
               return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditLog_Base")]
        public async Task<ActionResult<BitResultObject>> EditLog_Base(AddEditLogRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _LogRep.GetLogByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Log Log = new Log()
            {
                CreateDate = theRow.Result.CreateDate,
                ID = requestBody.ID,
                UpdateDate = DateTime.Now.ToShamsi(),
                ActionName = requestBody.ActionName,
                LogTime = requestBody.LogTime,
                Description = requestBody.Description,

            };
            result = await _LogRep.EditLogAsync(Log);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("DeleteLog_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteLog_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LogRep.RemoveLogAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
