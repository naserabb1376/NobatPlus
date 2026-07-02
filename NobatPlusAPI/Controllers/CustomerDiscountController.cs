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
using NobatPlusAPI.Models.CustomerDiscount;
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

namespace NobatPlusAPI.Controllers
{
    [Route("CustomerDiscount")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class CustomerDiscountController : ControllerBase
    {
        ICustomerDiscountRep _CustomerDiscountRep;
        ICustomerRep _CustomerRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public CustomerDiscountController(ICustomerDiscountRep CustomerDiscountRep,ICustomerRep CustomerRep,ILogRep logRep, IMapper mapper)
        {
           _CustomerDiscountRep = CustomerDiscountRep;
           _CustomerRep = CustomerRep;
           _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllCustomerDiscounts_Base")]
        public async Task<ActionResult<ListResultObject<CustomerDiscountVM>>> GetAllCustomerDiscounts_Base(GetCustomerDiscountListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            if (User.GetCurrentRoleId() != (long)DbTools.BaseRole.Admin)
            {
                var customerId = await GetCurrentCustomerIdAsync();
                if (customerId <= 0) return Forbid();
                requestBody.CustomerId = customerId;
            }
            var result = await _CustomerDiscountRep.GetAllCustomerDiscountsAsync(requestBody.DiscountId,requestBody.CustomerId,requestBody.StylistId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<CustomerDiscountVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetCustomerDiscountById_Base")]
        public async Task<ActionResult<RowResultObject<CustomerDiscountVM>>> GetCustomerDiscountById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerDiscountRep.GetCustomerDiscountByIdAsync(requestBody.ID);
            if (result.Status)
            {
                if (!CanAccessCustomerDiscount(result.Result))
                {
                    return Forbid();
                }

                var resultVM = _mapper.Map<RowResultObject<CustomerDiscountVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistCustomerDiscount_Base")]
        [RequireRole(4)]
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
        [RequireRole(4)]
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
        [RequireRole(4)]
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
        [RequireRole(4)]
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

        private async Task<long> GetCurrentCustomerIdAsync()
        {
            var result = await _CustomerRep.ExistCustomerAsync(User.GetCurrentUserId().ToString(), "personid");
            return result.Status ? result.ID : 0;
        }

        private bool CanAccessCustomerDiscount(CustomerDiscount? discount)
        {
            if (User.GetCurrentRoleId() == (long)DbTools.BaseRole.Admin)
                return true;

            var customer = _CustomerRep.ExistCustomerAsync(User.GetCurrentUserId().ToString(), "personid").GetAwaiter().GetResult();
            return customer.Status && discount?.CustomerId == customer.ID;
        }

    }
}
