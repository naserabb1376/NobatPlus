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
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
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

        public DiscountController(IDiscountRep DiscountRep,ILogRep logRep)
        {
           _DiscountRep = DiscountRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllDiscounts_Base")]
        public async Task<ActionResult<ListResultObject<Discount>>> GetAllDiscounts_Base(GetDiscountListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountRep.GetAllDiscountsAsync((DiscountType)requestBody.DiscountType,requestBody.AdminId,requestBody.StylistId,requestBody.CustomerId,requestBody.StylistId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetDiscountById_Base")]
        public async Task<ActionResult<RowResultObject<Discount>>> GetDiscountById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountRep.GetDiscountByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> ExistDiscount_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _DiscountRep.ExistDiscountAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> AddDiscount_Base(AddEditDiscountRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Discount Discount = new Discount()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                StartDate = requestBody.StartDate,
                EndDate = requestBody.EndDate,
                DiscountAmount = requestBody.DiscountAmount,
                DiscountCode = requestBody.DiscountCode,
                UpdateDate = DateTime.Now.ToShamsi(),
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
                DiscountCode = requestBody.DiscountCode,
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

        [HttpDelete("DeleteDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteDiscount_Base(GetRowRequestBody requestBody)
        {
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
