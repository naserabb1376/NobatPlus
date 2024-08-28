using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Admin;
using NobatPlusAPI.Models.Authenticate;
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
    [Route("Admin")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class AdminController : ControllerBase
    {
        IAdminRep _AdminRep;
        ILogRep _logRep;

        public AdminController(IAdminRep AdminRep,ILogRep logRep)
        {
           _AdminRep = AdminRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllAdmins_Base")]
        public async Task<ActionResult<ListResultObject<Admin>>> GetAllAdmins_Base(GetAdminListRequestBody requestBody)
        {
            ListResultObject<Admin> result = new ListResultObject<Admin>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            if (requestBody.DiscountId > 0)
            {
                result = await _AdminRep.GetAdminsOfDiscountAsync(requestBody.DiscountId,requestBody.CityId,requestBody.Role,requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText);
            }
            else
                result = await _AdminRep.GetAllAdminsAsync(requestBody.Role,requestBody.CityId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetAdminById_Base")]
        public async Task<ActionResult<ListResultObject<Admin>>> GetAdminById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _AdminRep.GetAdminByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistAdmin_Base")]
        public async Task<ActionResult<BitResultObject>> ExistAdmin_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _AdminRep.ExistAdminAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddAdmin_Base")]
        public async Task<ActionResult<BitResultObject>> AddAdmin_Base(AddEditAdminRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Admin Admin = new Admin()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                Role = requestBody.Role,
                PersonID = requestBody.PersonId,
                Description = requestBody.Description,
            };
            var result = await _AdminRep.AddAdminAsync(Admin);
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

        [HttpPut("EditAdmin_Base")]
        public async Task<ActionResult<BitResultObject>> EditAdmin_Base(AddEditAdminRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theRow = await _AdminRep.GetAdminByIdAsync(requestBody.ID);

            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Admin Admin = new Admin()
            {
                UpdateDate = DateTime.Now.ToShamsi(),
                 ID = requestBody.ID,
                 CreateDate = theRow.Result.CreateDate,
                 PersonID = requestBody.PersonId,
                 Role = requestBody.Role,
                 Description = requestBody.Description,
            };
             result = await _AdminRep.EditAdminAsync(Admin);
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

        [HttpDelete("DeleteAdmin_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteAdmin_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _AdminRep.RemoveAdminAsync(requestBody.ID);
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
