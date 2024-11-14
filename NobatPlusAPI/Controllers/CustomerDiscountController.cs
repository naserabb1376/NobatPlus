using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.City;
using NobatPlusAPI.Models.CustomerDiscount;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerDiscountRep.GetAllCustomerDiscountsAsync(requestBody.DiscountId,requestBody.CustomerId,requestBody.StylistId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetCustomerDiscountById_Base")]
        public async Task<ActionResult<RowResultObject<CustomerDiscount>>> GetCustomerDiscountById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
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
            if (!ModelState.IsValid)
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

        [HttpPost("AddCustomerDiscounts_Base")]
        public async Task<ActionResult<BitResultObject>> AddCustomerDiscounts_Base(List<AddEditCustomerDiscountRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var customerDiscounts = requestBodyList.Select(requestBody => new CustomerDiscount
            {
                CreateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description,
                StylistId = requestBody.StylistId,
                CustomerId = requestBody.CustomerId,
                DiscountId = requestBody.DiscountId,
                UpdateDate = DateTime.Now.ToShamsi(),
            }).ToList();

            var result = await _CustomerDiscountRep.AddCustomerDiscountsAsync(customerDiscounts);
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


        [HttpPut("EditCustomerDiscounts_Base")]
        public async Task<ActionResult<BitResultObject>> EditCustomerDiscounts_Base(List<AddEditCustomerDiscountRequestBody> requestBodyList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodyList);
            }

            var resultList = new List<CustomerDiscount>();

            foreach (var requestBody in requestBodyList)
            {
                var theRow = await _CustomerDiscountRep.GetCustomerDiscountByIdAsync(requestBody.ID);
                if (!theRow.Status)
                {
                    return BadRequest(theRow);
                }

                resultList.Add(new CustomerDiscount
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ID = requestBody.ID,
                    Description = requestBody.Description,
                    StylistId = requestBody.StylistId,
                    CustomerId = requestBody.CustomerId,
                    DiscountId = requestBody.DiscountId,
                });
            }

            var result = await _CustomerDiscountRep.EditCustomerDiscountsAsync(resultList);
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


        [HttpDelete("DeleteCustomerDiscounts_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteCustomerDiscounts_Base(List<long> ids)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ids);
            }

            var result = await _CustomerDiscountRep.RemoveCustomerDiscountsAsync(ids);
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
