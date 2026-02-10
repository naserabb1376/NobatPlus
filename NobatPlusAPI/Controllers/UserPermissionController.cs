using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.UserPermission;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MTPermissionCenter.EFCore.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static NobatPlusAPI.Tools.ToolBox;
using Domains;

namespace NobatPlusAPI.Controllers
{
    [Route("UserPermission")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    // [CheckRoleBase(new[] { (int)BaseRole.GeneralAdmin })]

    public class UserPermissionController : ControllerBase
    {
        IUserPermissionRep _UserPermissionRep;
        IPermissionInvalidationService _PermissionInvalidationService;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public UserPermissionController(IUserPermissionRep UserPermissionRep,ILogRep logRep,IPermissionInvalidationService permissionInvalidationService,IMapper mapper)
        {
           _UserPermissionRep = UserPermissionRep;
           _logRep = logRep;
            _PermissionInvalidationService = permissionInvalidationService;
            _mapper = mapper;
        }

        [HttpPost("GetAllUserPermissions_Base")]
        public async Task<ActionResult<ListResultObject<UserPermissionVM>>> GetAllUserPermissions_Base(GetUserPermissionListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _UserPermissionRep.GetAllUserPermissionsAsync(requestBody.UserId, requestBody.PermissionId, requestBody.PermissionType ?? "",requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<UserPermissionVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetUserPermissionById_Base")]
        public async Task<ActionResult<RowResultObject<UserPermissionVM>>> GetUserPermissionById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _UserPermissionRep.GetUserPermissionByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<UserPermissionVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistUserPermission_Base")]
        public async Task<ActionResult<BitResultObject>> ExistUserPermission_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _UserPermissionRep.ExistUserPermissionAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddUserPermissions_Base")]
        public async Task<ActionResult<BitResultObject>> AddUserPermissions_Base(List<AddEditUserPermissionRequestBody> requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var UserPermissions = requestBody.Select(x=> new MTPermissionCenter_UserPermission()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PermissionId = x.PermissionId,
                UserId = x.UserId,
                OwnerOnly = x.OwnerOnly,
                IsGranted = x.IsGranted,
                IsActive = true
            }).ToList();
            
            var result = await _UserPermissionRep.AddUserPermissionsAsync(UserPermissions);
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

                await _PermissionInvalidationService.BumpUserVersionAsync(UserPermissions.Select(x=> x.UserId).ToList());

                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditUserPermissions_Base")]
        public async Task<ActionResult<BitResultObject>> EditUserPermissions_Base(List<AddEditUserPermissionRequestBody> requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = new BitResultObject();
            var UserPermissions = new List<MTPermissionCenter_UserPermission>();

            foreach (var body in requestBody)
            {
                var theRow = await _UserPermissionRep.GetUserPermissionByIdAsync(body.ID);
                if (!theRow.Status)
                {
                    result.Status = theRow.Status;
                    result.ErrorMessage = theRow.ErrorMessage;
                    return BadRequest(result);
                }

                var UserPermission = new MTPermissionCenter_UserPermission
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ID = body.ID,
                    PermissionId = body.PermissionId,
                    OwnerOnly = body.OwnerOnly,
                    IsGranted = body.IsGranted,
                    IsActive = true,
                };

                UserPermissions.Add(UserPermission);
            }

            result = await _UserPermissionRep.EditUserPermissionsAsync(UserPermissions);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
                };
                await _logRep.AddLogAsync(log);

                #endregion

                await _PermissionInvalidationService.BumpUserVersionAsync(UserPermissions.Select(x => x.UserId).ToList());

                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteUserPermissions_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteUserPermissions_Base(List<long> ids)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ids);
            }

            var result = await _UserPermissionRep.RemoveUserPermissionsAsync(ids);
            if (result.Status)
            {
                #region AddLog

                Log log = new Log
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
                };
                await _logRep.AddLogAsync(log);

                #endregion

                await _PermissionInvalidationService.BumpUserVersionAsync(ids);

                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
