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
        ILogRep _logRep;
        IAddressRep _addressRep;

        public PersonController(IPersonRep PersonRep,IAddressRep addressRep,ILogRep logRep)
        {
           _PersonRep = PersonRep;
           _logRep = logRep;
           _addressRep = addressRep;
        }

        [HttpPost("GetAllPersons_Base")]
        public async Task<ActionResult<ListResultObject<Person>>> GetAllPersons_Base(GetPersonListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PersonRep.GetAllPersonsAsync(requestBody.CityId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetPersonById_Base")]
        public async Task<ActionResult<RowResultObject<Person>>> GetPersonById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PersonRep.GetPersonByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            Address address = new Address()
            {
                CityID = requestBody.CityID,
                AddressLocationHorizentalPoint = requestBody.AddressLocationHorizentalPoint,
                AddressLocationVerticalPoint = requestBody.AddressLocationVerticalPoint,
                AddressPostalCode = requestBody.AddressPostalCode,
                AddressStreet = requestBody.AddressStreet,
                Description = requestBody.AddressDescription,
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),

            };

            if (result.Status)
            {
                result = await _addressRep.AddAddressAsync(address);

                Person Person = new Person()
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    AddressID = address.ID,
                    DateOfBirth = requestBody.DateOfBirth?.StringToDate(),
                    FirstName = requestBody.FirstName,
                    LastName = requestBody.LastName,
                    Email = requestBody.Email,
                    NaCode = requestBody.NaCode,
                    PhoneNumber = requestBody.PhoneNumber,
                    Description = requestBody.Description,
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

            Address address = new Address()
            {
                CityID = requestBody.CityID,
                ID = theRow.Result.AddressID,
                AddressLocationHorizentalPoint = requestBody.AddressLocationHorizentalPoint,
                AddressLocationVerticalPoint = requestBody.AddressLocationVerticalPoint,
                AddressPostalCode = requestBody.AddressPostalCode,
                AddressStreet = requestBody.AddressStreet,
                Description = requestBody.AddressDescription,
                CreateDate = theRow.Result.Address.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),

            };

            if (result.Status)
            {
                result = await _addressRep.EditAddressAsync(address);

                Person Person = new Person()
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ID = requestBody.ID,
                    AddressID = address.ID,
                    DateOfBirth = requestBody.DateOfBirth?.StringToDate(),
                    FirstName = requestBody.FirstName,
                    LastName = requestBody.LastName,
                    Email = requestBody.Email,
                    NaCode = requestBody.NaCode,
                    PhoneNumber = requestBody.PhoneNumber,
                    Description = requestBody.Description,
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
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Person Person = new Person()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                AddressID = requestBody.AdressId,
                DateOfBirth = requestBody.DateOfBirth?.StringToDate(),
                FirstName = requestBody.FirstName,
                LastName = requestBody.LastName,
                Email = requestBody.Email,
                NaCode = requestBody.NaCode,
                PhoneNumber = requestBody.PhoneNumber,
                Description = requestBody.Description,
            };
            var result = await _PersonRep.AddPersonAsync(Person);
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
                AddressID = requestBody.AdressId,
                DateOfBirth = requestBody.DateOfBirth?.StringToDate(),
                FirstName = requestBody.FirstName,
                LastName = requestBody.LastName,
                Email = requestBody.Email,
                NaCode = requestBody.NaCode,
                PhoneNumber = requestBody.PhoneNumber,
                Description = requestBody.Description,
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
