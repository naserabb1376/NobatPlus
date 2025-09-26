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
using NobatPlusAPI.Models.ServiceDiscount;
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
    [Route("ServiceDiscount")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class ServiceDiscountController : ControllerBase
    {
        IServiceDiscountRep _ServiceDiscountRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public ServiceDiscountController(IServiceDiscountRep ServiceDiscountRep,ILogRep logRep, IMapper mapper)
        {
           _ServiceDiscountRep = ServiceDiscountRep;
           _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllServiceDiscounts_Base")]
        public async Task<ActionResult<ListResultObject<ServiceDiscountVM>>> GetAllServiceDiscounts_Base(GetServiceDiscountListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceDiscountRep.GetAllServiceDiscountsAsync(requestBody.DiscountID,requestBody.ServiceID,requestBody.AdminID,requestBody.StylistID,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<ServiceDiscountVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetServiceDiscountById_Base")]
        public async Task<ActionResult<RowResultObject<ServiceDiscountVM>>> GetServiceDiscountById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _ServiceDiscountRep.GetServiceDiscountByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<ServiceDiscountVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistServiceDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> ExistServiceDiscount_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
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

        [HttpPost("AddServiceDiscounts_Base")]
        public async Task<ActionResult<BitResultObject>> AddServiceDiscounts_Base(List<AddEditServiceDiscountRequestBody> requestBodies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodies);
            }

            var serviceDiscounts = requestBodies.Select(requestBody => new ServiceDiscount
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                DiscountId = requestBody.DiscountID,
                AdminId = requestBody.AdminID,
                ServiceManagementId = requestBody.ServiceID,
                StylistId = requestBody.StylistID,
                Description = requestBody.Description
            }).ToList();

            var result = await _ServiceDiscountRep.AddServiceDiscountsAsync(serviceDiscounts);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log
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


        [HttpPut("EditServiceDiscounts_Base")]
        public async Task<ActionResult<BitResultObject>> EditServiceDiscounts_Base(List<AddEditServiceDiscountRequestBody> requestBodies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodies);
            }

            var result = new BitResultObject();
            var serviceDiscounts = new List<ServiceDiscount>();

            foreach (var requestBody in requestBodies)
            {
                var theRow = await _ServiceDiscountRep.GetServiceDiscountByIdAsync(requestBody.ID);
                if (!theRow.Status)
                {
                    result.Status = theRow.Status;
                    result.ErrorMessage = theRow.ErrorMessage;
                    return BadRequest(result);
                }

                var serviceDiscount = new ServiceDiscount
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ID = requestBody.ID,
                    DiscountId = requestBody.DiscountID,
                    AdminId = requestBody.AdminID,
                    ServiceManagementId = requestBody.ServiceID,
                    StylistId = requestBody.StylistID,
                    Description = requestBody.Description
                };

                serviceDiscounts.Add(serviceDiscount);
            }

            result = await _ServiceDiscountRep.EditServiceDiscountsAsync(serviceDiscounts);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log
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

        [HttpDelete("DeleteServiceDiscounts_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteServiceDiscounts_Base(List<long> ids)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ids);
            }

            var result = await _ServiceDiscountRep.RemoveServiceDiscountsAsync(ids);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log
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
