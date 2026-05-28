using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.City;
using NobatPlusAPI.Models.Discount;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusAPI.Controllers
{
    [Route("Discount")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class DiscountController : ControllerBase
    {
        IDiscountRep _DiscountRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public DiscountController(IDiscountRep DiscountRep,ILogRep logRep, IMapper mapper)
        {
           _DiscountRep = DiscountRep;
           _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllDiscounts_Base")]
        public async Task<ActionResult<ListResultObject<DiscountVM>>> GetAllDiscounts_Base(GetDiscountListRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4) return Forbid();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountRep.GetAllDiscountsAsync((DiscountType)requestBody.DiscountType,requestBody.AdminId,requestBody.StylistId,requestBody.CustomerId,requestBody.StylistId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<DiscountVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetDiscountById_Base")]
        public async Task<ActionResult<RowResultObject<DiscountVM>>> GetDiscountById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountRep.GetDiscountByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<DiscountVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> ExistDiscount_Base(ExistDiscountRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountRep.ExistDiscountAsync(requestBody.ExistType,requestBody.KeyValue);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddDiscount")]
        public async Task<ActionResult<BitResultObject>> AddDiscount(AddEditDiscountRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4) return Forbid();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var discountAssignment = new DiscountAssignment
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,

                AdminId = requestBody.AdminId,
                StylistId = requestBody.StylistId,

            };

            var customerDiscounts = requestBody.CustomerIds.Select(cid => new CustomerDiscount()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,

              
                StylistId = requestBody.StylistId ?? 0,

                CustomerId = cid,

            }).ToList();

            var serviceDiscounts = requestBody.ServiceIds.Select(sid=> new ServiceDiscount()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,

                AdminId = requestBody.AdminId,
                StylistId = requestBody.StylistId,

                ServiceManagementId = sid,

            }).ToList();

            Discount Discount = new Discount()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                StartDate = requestBody.StartDate,
                EndDate = requestBody.EndDate,
                DiscountAmount = requestBody.DiscountAmount,
                DiscountCode = requestBody.DiscountCode.GenerateDiscountCode(),
                UpdateDate = DateTime.Now.ToShamsi(),
                CodeRequired = requestBody.CodeRequired,
                DiscountAssignments = new List<DiscountAssignment>()
                {
                    discountAssignment
                },
                CustomerDiscounts = customerDiscounts,
                ServiceDiscounts = serviceDiscounts,
            };
            var result = await _DiscountRep.AddDiscountAsync(Discount);
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

        [HttpPut("EditDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> EditDiscount_Base(AddEditDiscountRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4) return Forbid();
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _DiscountRep.GetDiscountByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Discount Discount = new Discount()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                Description = requestBody.Description,
                StartDate = requestBody.StartDate,
                EndDate = requestBody.EndDate,
                DiscountAmount = requestBody.DiscountAmount,
                DiscountCode = requestBody.DiscountCode.GenerateDiscountCode(),
            };
            result = await _DiscountRep.EditDiscountAsync(Discount);
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

        [HttpDelete("DeleteDiscount")]
        public async Task<ActionResult<BitResultObject>> DeleteDiscount(GetRowRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4) return Forbid();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountRep.RemoveDiscountAsync(requestBody.ID);
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
