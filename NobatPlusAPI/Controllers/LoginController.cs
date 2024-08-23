using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Login;
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
    [Route("Login")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class LoginController : ControllerBase
    {
        ILoginRep _LoginRep;
        ILogRep _logRep;

        public LoginController(ILoginRep LoginRep,ILogRep logRep)
        {
           _LoginRep = LoginRep;
           _logRep = logRep;
        }

        [HttpPost("GetAllLogins_Base")]
        public async Task<ActionResult<ListResultObject<Login>>> GetAllLogins_Base(GetLoginListRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LoginRep.GetAllLoginsAsync(requestBody.PersonId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("GetLoginById_Base")]
        public async Task<ActionResult<ListResultObject<Login>>> GetLoginById_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LoginRep.GetLoginByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("Authenticate_Base")]
        public async Task<ActionResult<ListResultObject<Login>>> Authenticate_Base(AuthenticateRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LoginRep.AuthenticateAsync(requestBody.Username,requestBody.Password);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }


        [HttpPost("ExistLogin_Base")]
        public async Task<ActionResult<BitResultObject>> ExistLogin_Base(ExistLoginRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LoginRep.ExistLoginAsync(requestBody.UniqueProperty,requestBody.SearchMode);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddLogin_Base")]
        public async Task<ActionResult<BitResultObject>> AddLogin_Base(AddEditLoginRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Login Login = new Login()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LastLoginDate = requestBody.LastLoginDate,
                PersonID = requestBody.PersonID,
                Username = requestBody.Username,
                PasswordHash = requestBody.Password.ToHash(),
                Description = requestBody.Description,
            };
            var result = await _LoginRep.AddLoginAsync(Login);
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

        [HttpPut("EditLogin_Base")]
        public async Task<ActionResult<BitResultObject>> EditLogin_Base(AddEditLoginRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _LoginRep.GetLoginByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Login Login = new Login()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                LastLoginDate = requestBody.LastLoginDate,
                PersonID = requestBody.PersonID,
                Username = requestBody.Username,
                PasswordHash = requestBody.Password.ToHash(),
                Description = requestBody.Description,
                ID = requestBody.ID,

            };
            result = await _LoginRep.EditLoginAsync(Login);
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

        [HttpDelete("DeleteLogin_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteLogin_Base(GetRowRequestBody requestBody)
        {
            if (ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _LoginRep.RemoveLoginAsync(requestBody.ID);
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
