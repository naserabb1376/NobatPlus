using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.RequestObjects;
using NobatPlusAPI.RequestObjects.Authenticate;
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
    [Route("Address")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class AddressController : ControllerBase
    {
        IAddressRep _AddressRep;
        ILogRep _logRep;

        public AddressController(IAddressRep AddressRep,ILogRep logRep)
        {
           _AddressRep = AddressRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllAddresses_Base")]
        public async Task<ActionResult<ListResultObject<Address>>> GetAllAddresses_Base(GetAddressListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _AddressRep.GetAllAddressesAsync(requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetAddressById_Base")]
        public async Task<ActionResult<ListResultObject<Address>>> GetAddressById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _AddressRep.GetAddressByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistAddress_Base")]
        public async Task<ActionResult<BitResultObject>> ExistAddress_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _AddressRep.ExistAddressAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddAddress_Base")]
        public async Task<ActionResult<BitResultObject>> AddAddress_Base(AddEditAddressRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Address Address = new Address()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                CityID = requestBody.CityID,
                AddressLocationHorizentalPoint = requestBody.AddressLocationHorizentalPoint,
                AddressLocationVerticalPoint = requestBody.AddressLocationVerticalPoint,
                AddressPostalCode = requestBody.AddressPostalCode,
                AddressStreet = requestBody.AddressStreet,
                Description = requestBody.AddressDescription,
            };
            var result = await _AddressRep.AddAddressAsync(Address);
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

        [HttpPut("EditAddress_Base")]
        public async Task<ActionResult<BitResultObject>> EditAddress_Base(AddEditAddressRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theRow = await _AddressRep.GetAddressByIdAsync(requestBody.ID);

            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Address Address = new Address()
            {
                UpdateDate = DateTime.Now.ToShamsi(),
                 ID = requestBody.ID,
                 CreateDate = theRow.Result.CreateDate,
                CityID = requestBody.CityID,
                AddressLocationHorizentalPoint = requestBody.AddressLocationHorizentalPoint,
                AddressLocationVerticalPoint = requestBody.AddressLocationVerticalPoint,
                AddressPostalCode = requestBody.AddressPostalCode,
                AddressStreet = requestBody.AddressStreet,
                Description = requestBody.AddressDescription,
            };
             result = await _AddressRep.EditAddressAsync(Address);
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

        [HttpDelete("DeleteAddress_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteAddress_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _AddressRep.RemoveAddressAsync(requestBody.ID);
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
