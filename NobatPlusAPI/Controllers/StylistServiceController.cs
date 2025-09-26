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
using NobatPlusAPI.Models.StylistService;
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
    [Route("StylistService")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class StylistServiceController : ControllerBase
    {
        IStylistServiceRep _StylistServiceRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public StylistServiceController(IStylistServiceRep StylistServiceRep,ILogRep logRep, IMapper mapper)
        {
           _StylistServiceRep = StylistServiceRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllStylistServices_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<StylistServiceVM>>> GetAllStylistServices_Base(GetStylistServiceListRequestBody requestBody)
        {
            var result = new ListResultObject<StylistService>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            result = await _StylistServiceRep.GetAllStylistServicesAsync(requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<StylistServiceVM>>(result);
                return Ok(resultVM);
            }

            return BadRequest(result);
        }


        [HttpPost("GetStylistServiceById_Base")]
        public async Task<ActionResult<RowResultObject<StylistServiceVM>>> GetStylistServiceById_Base(GetStylistServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _StylistServiceRep.GetStylistServiceByIdAsync(requestBody.StylistID,requestBody.ServiceID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<StylistServiceVM>>(result);
                return Ok(resultVM);
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

        [HttpPost("AddStylistServices_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylistServices_Base(List<AddEditStylistServiceRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var stylistServices = requestBodyList.Select(requestBody => new StylistService()
            {
                StylistID = requestBody.StylistID,
                ServiceManagementID = requestBody.ServiceID,
                DepositPercent = requestBody.DepositPercent,
                ServiceDuration = requestBody.Duration,
                ServicePrice = requestBody.ServicePrice,
            }).ToList();

            var result = await _StylistServiceRep.AddStylistServicesAsync(stylistServices);

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


        [HttpPut("EditStylistServices_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylistServices_Base(List<AddEditStylistServiceRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var stylistServices = requestBodyList.Select(requestBody => new StylistService()
            {
                StylistID = requestBody.StylistID,
                ServiceManagementID = requestBody.ServiceID,
                ServiceDuration = requestBody.Duration,
                DepositPercent = requestBody.DepositPercent,
                ServicePrice = requestBody.ServicePrice,
            }).ToList();

            var result = await _StylistServiceRep.EditStylistServicesAsync(stylistServices);

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


        [HttpDelete("DeleteStylistServices_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistServices_Base(List<GetStylistServiceRowRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var stylistServiceIds = requestBodyList
                .Select(requestBody => (requestBody.StylistID, requestBody.ServiceID))
                .ToList();

            var result = await _StylistServiceRep.RemoveStylistServicesAsync(stylistServiceIds);

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
