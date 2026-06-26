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
    [Route("Customer")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class CustomerController : ControllerBase
    {
        ICustomerRep _CustomerRep;
        IStylistRep _StylistRep;
        IPersonRep _PersonRep;
        IAddressRep _AddressRep;
        ILoginRep _LoginRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public CustomerController(
            ICustomerRep CustomerRep,
            IStylistRep stylistRep,
            IPersonRep personRep,
            IAddressRep addressRep,
            ILoginRep loginRep,
            ILogRep logRep,
            IMapper mapper)
        {
           _CustomerRep = CustomerRep;
            _StylistRep = stylistRep;
            _PersonRep = personRep;
            _AddressRep = addressRep;
            _LoginRep = loginRep;
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
         
            var result = await _CustomerRep.GetAllCustomersAsync(requestBody.StylistId, requestBody.CityId, requestBody.DiscountId, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery, requestBody.IsActive);
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
        [RequireRole(4)]
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
            if (User.GetCurrentRoleId() != 4)
            {
                requestBody.PersonID = User.GetCurrentUserId();
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

        [HttpPost("QuickAddCustomer")]
        public async Task<ActionResult<BitResultObject>> QuickAddCustomer(QuickAddCustomerRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var normalizedPhone = requestBody.PhoneNumber.Trim();
            var existingPerson = await _LoginRep.ExistLoginAsync(normalizedPhone, "PhoneNumber");

            if (existingPerson.Status && existingPerson.ID > 0)
            {
                var existingCustomer = await _CustomerRep.ExistCustomerAsync(
                    existingPerson.ID.ToString(),
                    "personid");

                if (existingCustomer.Status)
                    return Ok(existingCustomer);

                var customerResult = await _CustomerRep.AddCustomerAsync(new Customer
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    PersonID = existingPerson.ID,
                    Description = requestBody.Description ?? "ایجاد سریع از ثبت نوبت دستی"
                });

                return customerResult.Status ? Ok(customerResult) : BadRequest(customerResult);
            }

            long? addressId = null;
            if (requestBody.CityID > 0 && !string.IsNullOrWhiteSpace(requestBody.AddressStreet))
            {
                var addressResult = await _AddressRep.AddAddressAsync(new Address
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    CityID = requestBody.CityID.Value,
                    AddressStreet = requestBody.AddressStreet.Trim(),
                    AddressPostalCode = requestBody.AddressPostalCode?.Trim() ?? "",
                    AddressLocationHorizentalPoint = "",
                    AddressLocationVerticalPoint = "",
                    Description = requestBody.Description ?? ""
                });
                if (!addressResult.Status)
                    return BadRequest(addressResult);
                addressId = addressResult.ID;
            }

            var personResult = await _PersonRep.AddPersonAsync(new Person
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                FirstName = requestBody.FirstName.Trim(),
                LastName = requestBody.LastName?.Trim() ?? "",
                PhoneNumber = normalizedPhone,
                Email = "",
                Gender = requestBody.Gender is 1 or 2 ? requestBody.Gender : 1,
                RoleId = 1,
                AddressID = addressId,
                IsActive = true,
                PermissionsVersion = 1,
                DateOfBirth = (requestBody.DateOfBirth ?? DateTime.Now).ToShamsi(),
                Description = requestBody.Description ?? "ایجاد سریع از ثبت نوبت دستی"
            });

            if (!personResult.Status)
            {
                if (addressId.HasValue)
                    await _AddressRep.RemoveAddressAsync(addressId.Value);
                return BadRequest(personResult);
            }

            var result = await _CustomerRep.AddCustomerAsync(new Customer
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PersonID = personResult.ID,
                Description = requestBody.Description ?? "ایجاد سریع از ثبت نوبت دستی"
            });

            if (!result.Status)
            {
                await _PersonRep.RemovePersonAsync(personResult.ID);
                if (addressId.HasValue)
                    await _AddressRep.RemoveAddressAsync(addressId.Value);
            }

            return result.Status ? Ok(result) : BadRequest(result);
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
                 PersonID = User.GetCurrentRoleId() == 4 ? requestBody.PersonID : theRow.Result.PersonID,
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
        [RequireRole(4)]
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
