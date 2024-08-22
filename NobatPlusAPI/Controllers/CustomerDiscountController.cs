using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.RequestObjects;
using NobatPlusAPI.RequestObjects.City;
using NobatPlusAPI.RequestObjects.Public;
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
    [Route("CustomerDiscount")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class CustomerDiscountController : ControllerBase
    {
        ICustomerDiscountRep _CustomerDiscountRep;
        ILogRep _logRep;

        public CustomerDiscountController(ICustomerDiscountRep CustomerDiscountRep,ILogRep logRep)
        {
           _CustomerDiscountRep = CustomerDiscountRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllCustomerDiscounts_Base")]
        public async Task<ActionResult<ListResultObject<CustomerDiscount>>> GetAllCustomerDiscounts_Base(GetCustomerDiscountListRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerDiscountRep.GetAllCustomerDiscountsAsync(requestBody.DiscountId,requestBody.CustomerId,requestBody.StylistId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetCustomerDiscountById_Base")]
        public async Task<ActionResult<ListResultObject<CustomerDiscount>>> GetCustomerDiscountById_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerDiscountRep.GetCustomerDiscountByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistCustomerDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> ExistCustomerDiscount_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerDiscountRep.ExistCustomerDiscountAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddCustomerDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> AddCustomerDiscount_Base(AddEditCustomerDiscountRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            CustomerDiscount CustomerDiscount = new CustomerDiscount()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                StylistId = requestBody.StylistId,
                CustomerId = requestBody.CustomerId,
                DiscountId = requestBody.DiscountId,
                UpdateDate = DateTime.Now.ToShamsi(),
            };
            var result = await _CustomerDiscountRep.AddCustomerDiscountAsync(CustomerDiscount);
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

        [HttpPut("EditCustomerDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> EditCustomerDiscount_Base(AddEditCustomerDiscountRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _CustomerDiscountRep.GetCustomerDiscountByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            CustomerDiscount CustomerDiscount = new CustomerDiscount()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                Description = requestBody.Description,
                StylistId = requestBody.StylistId,
                CustomerId = requestBody.CustomerId,
                DiscountId = requestBody.DiscountId,
            };
            result = await _CustomerDiscountRep.EditCustomerDiscountAsync(CustomerDiscount);
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

        [HttpDelete("DeleteCustomerDiscount_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteCustomerDiscount_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerDiscountRep.RemoveCustomerDiscountAsync(requestBody.ID);
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
