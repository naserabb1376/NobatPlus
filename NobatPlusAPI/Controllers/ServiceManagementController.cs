using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.ServiceManagement;
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
    [Route("ServiceManagement")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class ServiceManagementController : ControllerBase
    {
        IServiceManagementRep _ServiceManagementRep;
        ILogRep _logRep;

        public ServiceManagementController(IServiceManagementRep ServiceManagementRep,ILogRep logRep)
        {
           _ServiceManagementRep = ServiceManagementRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllServiceManagements_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<ServiceManagement>>> GetAllServiceManagements_Base(GetServiceManagementListRequestBody requestBody)
        {
            ListResultObject<ServiceManagement> result = new ListResultObject<ServiceManagement>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            if (requestBody.DiscountID > 0)
            {
               result = await _ServiceManagementRep.GetServicesOfDiscountAsync(requestBody.DiscountID,requestBody.ServiceGender?? ' ', requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            }
            if (requestBody.BookingID > 0)
            {
                result = await _ServiceManagementRep.GetServicesOfBookingAsync(requestBody.BookingID, requestBody.ServiceGender ?? ' ', requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            }
            if (requestBody.StylistID > 0)
            {
                result = await _ServiceManagementRep.GetServicesOfStylistAsync(requestBody.StylistID, requestBody.ServiceGender ?? ' ', requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            }
            else
                result = await _ServiceManagementRep.GetAllServiceManagementsAsync(requestBody.ParentID,requestBody.ServiceGender ?? ' ', requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetServiceManagementById_Base")]
        public async Task<ActionResult<RowResultObject<ServiceManagement>>> GetServiceManagementById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceManagementRep.GetServiceManagementByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistServiceManagement_Base")]
        public async Task<ActionResult<BitResultObject>> ExistServiceManagement_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceManagementRep.ExistServiceManagementAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddServiceManagement_Base")]
        public async Task<ActionResult<BitResultObject>> AddServiceManagement_Base(AddEditServiceManagementRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            ServiceManagement ServiceManagement = new ServiceManagement()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
               // Duration = requestBody.Duration,
                ServiceName = requestBody.ServiceName,
                ServiceParentID = requestBody.ServiceParentID,
                Description = requestBody.Description,
                ServiceGender = requestBody.ServiceGender,
            };
            var result = await _ServiceManagementRep.AddServiceManagementAsync(ServiceManagement);
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

        [HttpPut("EditServiceManagement_Base")]
        public async Task<ActionResult<BitResultObject>> EditServiceManagement_Base(AddEditServiceManagementRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _ServiceManagementRep.GetServiceManagementByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            ServiceManagement ServiceManagement = new ServiceManagement()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                //Duration = requestBody.Duration,
                ServiceName = requestBody.ServiceName,
                ServiceParentID = requestBody.ServiceParentID,
                Description = requestBody.Description,
                ServiceGender = requestBody.ServiceGender,
            };
            result = await _ServiceManagementRep.EditServiceManagementAsync(ServiceManagement);
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

        [HttpDelete("DeleteServiceManagement_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteServiceManagement_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceManagementRep.RemoveServiceManagementAsync(requestBody.ID);
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
