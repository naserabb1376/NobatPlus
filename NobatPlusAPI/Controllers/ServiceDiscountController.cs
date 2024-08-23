using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.ServiceDiscount;
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
    [Route("ServiceDiscount")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class ServiceDiscountController : ControllerBase
    {
        IServiceDiscountRep _ServiceDiscountRep;
        ILogRep _logRep;

        public ServiceDiscountController(IServiceDiscountRep ServiceDiscountRep,ILogRep logRep)
        {
           _ServiceDiscountRep = ServiceDiscountRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllServiceDiscounts_Base")]
        public async Task<ActionResult<ListResultObject<ServiceDiscount>>> GetAllServiceDiscounts_Base(GetServiceDiscountListRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceDiscountRep.GetAllServiceDiscountsAsync(requestBody.DiscountID,requestBody.ServiceID,requestBody.AdminID,requestBody.StylistID,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetServiceDiscountById_Base")]
        public async Task<ActionResult<ListResultObject<ServiceDiscount>>> GetServiceDiscountById_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceDiscountRep.GetServiceDiscountByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistServiceDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> ExistServiceDiscount_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceDiscountRep.ExistServiceDiscountAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddServiceDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> AddServiceDiscount_Base(AddEditServiceDiscountRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            ServiceDiscount ServiceDiscount = new ServiceDiscount()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                DiscountId = requestBody.DiscountID,
                AdminId = requestBody.AdminID,
                ServiceManagementId = requestBody.ServiceID,
                StylistId = requestBody.StylistID,
                Description = requestBody.Description,
            };
            var result = await _ServiceDiscountRep.AddServiceDiscountAsync(ServiceDiscount);
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

        [HttpPut("EditServiceDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> EditServiceDiscount_Base(AddEditServiceDiscountRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _ServiceDiscountRep.GetServiceDiscountByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            ServiceDiscount ServiceDiscount = new ServiceDiscount()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                DiscountId = requestBody.DiscountID,
                AdminId = requestBody.AdminID,
                ServiceManagementId = requestBody.ServiceID,
                StylistId = requestBody.StylistID,
                Description = requestBody.Description,
            };
            result = await _ServiceDiscountRep.EditServiceDiscountAsync(ServiceDiscount);
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

        [HttpDelete("DeleteServiceDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteServiceDiscount_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceDiscountRep.RemoveServiceDiscountAsync(requestBody.ID);
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
