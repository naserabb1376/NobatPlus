using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.CheckAvailability;
using NobatPlusAPI.Models.City;
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
    [Route("CheckAvailability")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class CheckAvailabilityController : ControllerBase
    {
        ICheckAvailabilityRep _CheckAvailabilityRep;
        ILogRep _logRep;

        public CheckAvailabilityController(ICheckAvailabilityRep CheckAvailabilityRep,ILogRep logRep)
        {
           _CheckAvailabilityRep = CheckAvailabilityRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllCheckAvailabilities_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<CheckAvailability>>> GetAllCheckAvailabilities_Base(GetCheckAvailabilityListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CheckAvailabilityRep.GetAllCheckAvailabilitiesAsync(requestBody.StylistId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetCheckAvailabilityById_Base")]
        public async Task<ActionResult<RowResultObject<CheckAvailability>>> GetCheckAvailabilityById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CheckAvailabilityRep.GetCheckAvailabilityByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistCheckAvailability_Base")]
        public async Task<ActionResult<BitResultObject>> ExistCheckAvailability_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CheckAvailabilityRep.ExistCheckAvailabilityAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddCheckAvailability_Base")]
        public async Task<ActionResult<BitResultObject>> AddCheckAvailability_Base(AddEditCheckAvailabilityRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            CheckAvailability CheckAvailability = new CheckAvailability()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                Date = requestBody.Date,
                Time = requestBody.Time,
                Description = requestBody.Description,
                StylistID = requestBody.StylistID,
                UpdateDate = DateTime.Now.ToShamsi(),
            };
            var result = await _CheckAvailabilityRep.AddCheckAvailabilityAsync(CheckAvailability);
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

        [HttpPut("EditCheckAvailability_Base")]
        public async Task<ActionResult<BitResultObject>> EditCheckAvailability_Base(AddEditCheckAvailabilityRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _CheckAvailabilityRep.GetCheckAvailabilityByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            CheckAvailability CheckAvailability = new CheckAvailability()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                Date = requestBody.Date,
                Time = requestBody.Time,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description,
            };
            result = await _CheckAvailabilityRep.EditCheckAvailabilityAsync(CheckAvailability);
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

        [HttpDelete("DeleteCheckAvailability_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteCheckAvailability_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CheckAvailabilityRep.RemoveCheckAvailabilityAsync(requestBody.ID);
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
