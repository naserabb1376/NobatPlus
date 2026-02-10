using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.PermissionRole;
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
    [Route("PermissionRole")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    // [CheckRoleBase(new[] { (int)BaseRole.GeneralAdmin })]

    public class PermissionRoleController : ControllerBase
    {
        IPermissionRoleRep _PermissionRoleRep;
        IPermissionInvalidationService _PermissionInvalidationService;
        ILogRep _logRep;
        private readonly IMapper _mapper;


        public PermissionRoleController(IPermissionRoleRep PermissionRoleRep,ILogRep logRep,IPermissionInvalidationService permissionInvalidationService,IMapper mapper)
        {
           _PermissionRoleRep = PermissionRoleRep;
           _logRep = logRep;
            _PermissionInvalidationService = permissionInvalidationService;
            _mapper = mapper;
        }

        [HttpPost("GetAllPermissionRoles_Base")]
        public async Task<ActionResult<ListResultObject<PermissionRoleVM>>> GetAllPermissionRoles_Base(GetPermissionRoleListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PermissionRoleRep.GetAllPermissionRolesAsync(requestBody.RoleId, requestBody.PermissionId, requestBody.PermissionType ?? "",requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<PermissionRoleVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetPermissionRoleById_Base")]
        public async Task<ActionResult<RowResultObject<PermissionRoleVM>>> GetPermissionRoleById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PermissionRoleRep.GetPermissionRoleByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<PermissionRoleVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistPermissionRole_Base")]
        public async Task<ActionResult<BitResultObject>> ExistPermissionRole_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PermissionRoleRep.ExistPermissionRoleAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddPermissionRoles_Base")]
        public async Task<ActionResult<BitResultObject>> AddPermissionRoles_Base(List<AddEditPermissionRoleRequestBody> requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var PermissionRoles = requestBody.Select(x=> new MTPermissionCenter_PermissionRole()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                PermissionId = x.PermissionId,
                RoleId = x.RoleId,
                OwnerOnly = x.OwnerOnly,
                IsActive = true
            }).ToList();
            
            var result = await _PermissionRoleRep.AddPermissionRolesAsync(PermissionRoles);
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

                await _PermissionInvalidationService.BumpRoleUsersVersionAsync(PermissionRoles.Select(x=> x.RoleId).ToList());

                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("EditPermissionRoles_Base")]
        public async Task<ActionResult<BitResultObject>> EditPermissionRoles_Base(List<AddEditPermissionRoleRequestBody> requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = new BitResultObject();
            var PermissionRoles = new List<MTPermissionCenter_PermissionRole>();

            foreach (var body in requestBody)
            {
                var theRow = await _PermissionRoleRep.GetPermissionRoleByIdAsync(body.ID);
                if (!theRow.Status)
                {
                    result.Status = theRow.Status;
                    result.ErrorMessage = theRow.ErrorMessage;
                    return BadRequest(result);
                }

                var PermissionRole = new MTPermissionCenter_PermissionRole
                {
                    CreateDate = theRow.Result.CreateDate,
                    UpdateDate = DateTime.Now.ToShamsi(),
                    ID = body.ID,
                    PermissionId = body.PermissionId,
                    RoleId = body.RoleId,
                    IsActive = true,
                    OwnerOnly = body.OwnerOnly,
                };

                PermissionRoles.Add(PermissionRole);
            }

            result = await _PermissionRoleRep.EditPermissionRolesAsync(PermissionRoles);
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

                await _PermissionInvalidationService.BumpRoleUsersVersionAsync(PermissionRoles.Select(x => x.RoleId).ToList());

                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeletePermissionRoles_Base")]
        public async Task<ActionResult<BitResultObject>> DeletePermissionRoles_Base(List<long> ids)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ids);
            }

            var result = await _PermissionRoleRep.RemovePermissionRolesAsync(ids);
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

                await _PermissionInvalidationService.BumpRoleUsersVersionAsync(ids);

                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
