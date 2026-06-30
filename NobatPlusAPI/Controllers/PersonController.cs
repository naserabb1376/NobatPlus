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
using NobatPlusAPI.Models.Person;
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
    [Route("Person")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class PersonController : ControllerBase
    {
        IPersonRep _PersonRep;
        ILoginRep _loginRep;
        ILogRep _logRep;
        IAddressRep _addressRep;
        private readonly IMapper _mapper;


        public PersonController(IPersonRep PersonRep,ILoginRep loginRep,IAddressRep addressRep,ILogRep logRep, IMapper mapper)
        {
           _PersonRep = PersonRep;
            _loginRep = loginRep;
           _logRep = logRep;
           _addressRep = addressRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllPersons_Base")]
        [AllowAnonymous]
        public async Task<ActionResult<ListResultObject<PersonVM>>> GetAllPersons_Base(GetPersonListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PersonRep.GetAllPersonsAsync(requestBody.CityId, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery, requestBody.RoleId, requestBody.IsActive);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<PersonVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetPersonById_Base")]
        public async Task<ActionResult<RowResultObject<PersonVM>>> GetPersonById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PersonRep.GetPersonByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<PersonVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistPerson_Base")]
        public async Task<ActionResult<BitResultObject>> ExistPerson_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PersonRep.ExistPersonAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }


        [HttpPost("AddPerson")]
        public async Task<ActionResult<BitResultObject>> AddPerson(AddEditPersonProRequestBody requestBody)
        {
            BitResultObject result = new BitResultObject();
            Address address = null;
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var validUserName = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");

            if (validUserName.Status)
            {
                result.Status = !validUserName.Status;
                result.ErrorMessage = "نام کاربری (شماره موبایل) تکراری است";
                return BadRequest(result);
            }

            var validPhoneNumber = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");

            if (validPhoneNumber.Status)
            {
                result.Status = !validPhoneNumber.Status;
                result.ErrorMessage = "شماره موبایل تکراری است";
                return BadRequest(result);
            }

            var validNaCode = await _loginRep.ExistLoginAsync(requestBody.NaCode, "NationalCode");

            if (validNaCode.Status)
            {
                result.Status = !validNaCode.Status;
                result.ErrorMessage = "کد ملی تکراری است";
                return BadRequest(result);
            }

            if (requestBody.Address != null)
            {
                address = new Address()
                {
                    CityID = requestBody.Address.CityID,
                    AddressLocationHorizentalPoint = requestBody.Address.AddressLocationHorizentalPoint,
                    AddressLocationVerticalPoint = requestBody.Address.AddressLocationVerticalPoint,
                    AddressPostalCode = requestBody.Address.AddressPostalCode ?? "",
                    AddressStreet = requestBody.Address.AddressStreet,
                    Description = requestBody.Address.AddressDescription,
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),

                };

                result = await _addressRep.AddAddressAsync(address);
            }
          

            if (result.Status)
            {

                Person Person = new Person()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    AddressID = (address != null && address.ID > 0) ? address.ID : null,
                    DateOfBirth = requestBody.DateOfBirth.StringToDate(),
                    FirstName = requestBody.FirstName,
                    LastName = requestBody.LastName,
                    Email = requestBody.Email ?? "",
                    NaCode = requestBody.NaCode,
                    PhoneNumber = requestBody.PhoneNumber,
                    Description = requestBody.Description,
                    Gender = requestBody.Gender,
                    RoleId = requestBody.RoleId,
                    IsActive = requestBody.IsActive,
                    PermissionsVersion = 1,
                    
                };
                result = await _PersonRep.AddPersonAsync(Person);
            }

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

        [HttpPut("EditPerson")]
        public async Task<ActionResult<BitResultObject>> EditPerson(AddEditPersonProRequestBody requestBody)
        {
            var result = new BitResultObject();
            Address address = null;
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var validUserName = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber",requestBody.ID);

            if (validUserName.Status)
            {
                result.Status = !validUserName.Status;
                result.ErrorMessage = "نام کاربری (شماره موبایل) تکراری است";
                return BadRequest(result);
            }

            var validPhoneNumber = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber", requestBody.ID);

            if (validPhoneNumber.Status)
            {
                result.Status = !validPhoneNumber.Status;
                result.ErrorMessage = "شماره موبایل تکراری است";
                return BadRequest(result);
            }

            var validNaCode = await _loginRep.ExistLoginAsync(requestBody.NaCode, "NationalCode", requestBody.ID);

            if (validNaCode.Status)
            {
                result.Status = !validNaCode.Status;
                result.ErrorMessage = "کد ملی تکراری است";
                return BadRequest(result);
            }

            var theRow = await _PersonRep.GetPersonByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }
            if (theRow.Status && theRow.Result.RoleId == 4 && theRow.Result.IsActive && (requestBody.RoleId != 4 || !requestBody.IsActive))
            {
                var activeAdmins = await _PersonRep.GetAllPersonsAsync(0, 1, 2, "", "", 4, true);
                if (activeAdmins.Status && activeAdmins.TotalCount <= 1)
                {
                    result.Status = false;
                    result.ErrorMessage = "آخرین ادمین فعال را نمی توان غیرفعال کرد یا نقش آن را تغییر داد.";
                    return BadRequest(result);
                }
            }
            if (requestBody.Address != null)
            {
                address = new Address()
                {
                    CityID = requestBody.Address.CityID,
                    ID = (theRow.Result.Address != null && theRow.Result.Address.ID > 0) ? theRow.Result.Address.ID : 0,
                    AddressLocationHorizentalPoint = requestBody.Address.AddressLocationHorizentalPoint,
                    AddressLocationVerticalPoint = requestBody.Address.AddressLocationVerticalPoint,
                    AddressPostalCode = requestBody.Address.AddressPostalCode ?? "",
                    AddressStreet = requestBody.Address.AddressStreet,
                    Description = requestBody.Address.AddressDescription,
                    CreateDate = (theRow.Result.Address != null && theRow.Result.Address.ID > 0) ? theRow.Result.Address.CreateDate : DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),

                };

                result = await _addressRep.EditAddressAsync(address);
            }
           
            if (result.Status)
            {

                Person Person = new Person()
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ID = requestBody.ID,
                    AddressID = address != null ? address.ID : null,
                    DateOfBirth = requestBody.DateOfBirth.StringToDate(),
                    FirstName = requestBody.FirstName,
                    LastName = requestBody.LastName,
                    Email = requestBody.Email ??"",
                    NaCode = requestBody.NaCode,
                    PhoneNumber = requestBody.PhoneNumber,
                    Description = requestBody.Description,
                    Gender = requestBody.Gender,
                    RoleId = requestBody.RoleId,
                    IsActive = requestBody.IsActive,
                    IdentificationCode = theRow.Result.IdentificationCode,
                    PermissionsVersion = theRow.Result.PermissionsVersion
                };
                result = await _PersonRep.EditPersonAsync(Person);
            }

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


        [HttpPost("AddPerson_Base")]
        public async Task<ActionResult<BitResultObject>> AddPerson_Base(AddEditPersonRequestBody requestBody)
        {
            BitResultObject result = new BitResultObject();

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var validUserName = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");

            if (validUserName.Status)
            {
                result.Status = !validUserName.Status;
                result.ErrorMessage = "نام کاربری (شماره موبایل) تکراری است";
                return BadRequest(result);
            }

            var validPhoneNumber = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");

            if (validPhoneNumber.Status)
            {
                result.Status = !validPhoneNumber.Status;
                result.ErrorMessage = "شماره موبایل تکراری است";
                return BadRequest(result);
            }

            var validNaCode = await _loginRep.ExistLoginAsync(requestBody.NaCode, "NationalCode");

            if (validNaCode.Status)
            {
                result.Status = !validNaCode.Status;
                result.ErrorMessage = "کد ملی تکراری است";
                return BadRequest(result);
            }

            Person Person = new Person()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                AddressID = (requestBody.AdressId > 0) ? requestBody.AdressId : null,
                DateOfBirth = requestBody.DateOfBirth.StringToDate(),
                FirstName = requestBody.FirstName,
                LastName = requestBody.LastName,
                Email = requestBody.Email ?? "",
                NaCode = requestBody.NaCode,
                PhoneNumber = requestBody.PhoneNumber,
                Description = requestBody.Description,
                Gender = requestBody.Gender,
                RoleId = requestBody.RoleId,
                IsActive = requestBody.IsActive,
                PermissionsVersion = 1,
            };
            result = await _PersonRep.AddPersonAsync(Person);
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

        [HttpPut("EditPerson_Base")]
        public async Task<ActionResult<BitResultObject>> EditPerson_Base(AddEditPersonRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var validUserName = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber", requestBody.ID);

            if (validUserName.Status)
            {
                result.Status = !validUserName.Status;
                result.ErrorMessage = "نام کاربری (شماره موبایل) تکراری است";
                return BadRequest(result);
            }

            var validPhoneNumber = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber", requestBody.ID);

            if (validPhoneNumber.Status)
            {
                result.Status = !validPhoneNumber.Status;
                result.ErrorMessage = "شماره موبایل تکراری است";
                return BadRequest(result);
            }

            var validNaCode = await _loginRep.ExistLoginAsync(requestBody.NaCode, "NationalCode", requestBody.ID);

            if (validNaCode.Status)
            {
                result.Status = !validNaCode.Status;
                result.ErrorMessage = "کد ملی تکراری است";
                return BadRequest(result);
            }

            var theRow = await _PersonRep.GetPersonByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Person Person = new Person()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                AddressID = (requestBody.AdressId > 0) ? requestBody.AdressId : null,
                DateOfBirth = requestBody.DateOfBirth.StringToDate(),
                FirstName = requestBody.FirstName,
                LastName = requestBody.LastName,
                Email = requestBody.Email ?? "",
                NaCode = requestBody.NaCode,
                PhoneNumber = requestBody.PhoneNumber,
                Description = requestBody.Description,
                Gender = requestBody.Gender,
                RoleId = requestBody.RoleId,
                IsActive = requestBody.IsActive,
                PermissionsVersion = theRow.Result.PermissionsVersion,
                IdentificationCode = theRow.Result.IdentificationCode,
            };
            result = await _PersonRep.EditPersonAsync(Person);
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
        [HttpPut("DeactivatePerson")]
        public async Task<ActionResult<BitResultObject>> DeactivatePerson(DeactivatePersonRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _PersonRep.GetPersonByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Person Person = theRow.Result;
            if (Person.RoleId == 4 && Person.IsActive && !requestBody.IsActive)
            {
                var activeAdmins = await _PersonRep.GetAllPersonsAsync(0, 1, 2, "", "", 4, true);
                if (activeAdmins.Status && activeAdmins.TotalCount <= 1)
                {
                    result.Status = false;
                    result.ErrorMessage = "آخرین ادمین فعال را نمی توان غیرفعال کرد.";
                    return BadRequest(result);
                }
            }
            Person.IsActive = requestBody.IsActive;

            result = await _PersonRep.EditPersonAsync(Person);
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

        [HttpDelete("DeletePerson_Base")]
        public async Task<ActionResult<BitResultObject>> DeletePerson_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PersonRep.RemovePersonAsync(requestBody.ID);
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
