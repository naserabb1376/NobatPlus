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
using NobatPlusAPI.Models.City;
using NobatPlusAPI.Models.Customer;
using NobatPlusAPI.Models.Public;
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
    [Route("Customer")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class CustomerController : ControllerBase
    {
        ICustomerRep _CustomerRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public CustomerController(ICustomerRep CustomerRep,ILogRep logRep, IMapper mapper)
        {
           _CustomerRep = CustomerRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllCustomers_Base")]
        public async Task<ActionResult<ListResultObject<CustomerVM>>> GetAllCustomers_Base(GetCustomerListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerRep.GetAllCustomersAsync(requestBody.StylistId,requestBody.CityId,requestBody.DiscountId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<CustomerVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }


        [HttpPost("GetCustomerById_Base")]
        public async Task<ActionResult<RowResultObject<CustomerVM>>> GetCustomerById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerRep.GetCustomerByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<CustomerVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistCustomer_Base")]
        public async Task<ActionResult<BitResultObject>> ExistCustomer_Base(ExistCustomerRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerRep.ExistCustomerAsync(requestBody.FieldValue,requestBody.FieldName);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddCustomer_Base")]
        public async Task<ActionResult<BitResultObject>> AddCustomer_Base(AddEditCustomerRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Customer Customer = new Customer()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PersonID = requestBody.PersonID,
                Description = requestBody.Description,
            };
            var result = await _CustomerRep.AddCustomerAsync(Customer);
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

        [HttpPut("EditCustomer_Base")]
        public async Task<ActionResult<BitResultObject>> EditCustomer_Base(AddEditCustomerRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theRow = await _CustomerRep.GetCustomerByIdAsync(requestBody.ID);

            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Customer Customer = new Customer()
            {
                 UpdateDate = DateTime.Now.ToShamsi(),
                 ID = requestBody.ID,
                 CreateDate = theRow.Result.CreateDate,
                 PersonID = requestBody.PersonID,
                 Description = requestBody.Description,
            };
             result = await _CustomerRep.EditCustomerAsync(Customer);
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

        [HttpDelete("DeleteCustomer_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteCustomer_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _CustomerRep.RemoveCustomerAsync(requestBody.ID);
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
