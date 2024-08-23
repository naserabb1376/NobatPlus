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
using NobatPlusAPI.Models.StylistService;
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
    [Route("StylistService")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class StylistServiceController : ControllerBase
    {
        IStylistServiceRep _StylistServiceRep;
        ILogRep _logRep;

        public StylistServiceController(IStylistServiceRep StylistServiceRep,ILogRep logRep)
        {
           _StylistServiceRep = StylistServiceRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllStylistServices_Base")]
        public async Task<ActionResult<ListResultObject<StylistService>>> GetAllStylistServices_Base(GetStylistServiceListRequestBody requestBody)
        {
            var result = new ListResultObject<StylistService>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            result = await _StylistServiceRep.GetAllStylistServicesAsync(requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpPost("GetStylistServiceById_Base")]
        public async Task<ActionResult<ListResultObject<StylistService>>> GetStylistServiceById_Base(GetStylistServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistServiceRep.GetStylistServiceByIdAsync(requestBody.StylistID,requestBody.ServiceID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistStylistService_Base")]
        public async Task<ActionResult<BitResultObject>> ExistStylistService_Base(GetStylistServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistServiceRep.ExistStylistServiceAsync(requestBody.StylistID, requestBody.ServiceID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddStylistService_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylistService_Base(GetStylistServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            StylistService StylistService = new StylistService()
            {
               StylistID = requestBody.StylistID,
               ServiceManagementID = requestBody.ServiceID,
            };
            var result = await _StylistServiceRep.AddStylistServiceAsync(StylistService);
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
                result = await _logRep.AddLogAsync(log);

                #endregion


                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditStylistService_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylistService_Base(GetStylistServiceRowRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theRow = await _StylistServiceRep.GetStylistServiceByIdAsync(requestBody.StylistID,requestBody.ServiceID);

            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            StylistService StylistService = new StylistService()
            {
               StylistID = requestBody.StylistID,
               ServiceManagementID = requestBody.ServiceID,
            };
             result = await _StylistServiceRep.EditStylistServiceAsync(StylistService);
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

        [HttpDelete("DeleteStylistService_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistService_Base(GetStylistServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistServiceRep.RemoveStylistServiceAsync(requestBody.StylistID, requestBody.ServiceID);
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
