using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Permission;
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
    [Route("Permission")]
    [ApiController]
    [Produces("application/json")]
    [Authorize]
    // [CheckRoleBase(new[] { (int)BaseRole.GeneralAdmin })]

    public class PermissionController : ControllerBase
    {
        private IPermissionRep _PermissionRep;
        private IPermissionRoleRep _PermissionRoleRep;
        IPermissionInvalidationService _PermissionInvalidationService;
        private ILogRep _logRep;
        private readonly IMapper _mapper;


        public PermissionController(IPermissionRep PermissionRep,IPermissionRoleRep permissionRoleRep, ILogRep logRep,IPermissionInvalidationService permissionInvalidationService,IMapper mapper)
        {
            _PermissionRep = PermissionRep;
            _PermissionRoleRep = permissionRoleRep;
            _logRep = logRep;
            _PermissionInvalidationService = permissionInvalidationService;
            _mapper = mapper;
        }

        [HttpPost("GetAllPermissions_Base")]
        public async Task<ActionResult<ListResultObject<PermissionVM>>> GetAllPermissions_Base(GetPermissionListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var roleId = requestBody.RoleId ?? User.GetCurrentRoleId();
            var userId = requestBody.UserId ?? User.GetCurrentUserId();

            var result = await _PermissionRep.GetAllPermissionsAsync(roleId, userId, requestBody.PermissionType ?? "menu",requestBody.MenuParentId,requestBody.MenuIds, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<PermissionVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetPermissionById_Base")]
        public async Task<ActionResult<RowResultObject<PermissionVM>>> GetPermissionById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PermissionRep.GetPermissionByIdAsync(requestBody.ID);
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<PermissionVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistPermission_Base")]
        public async Task<ActionResult<BitResultObject>> ExistPermission_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PermissionRep.ExistPermissionAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddPermission_Base")]
        public async Task<ActionResult<BitResultObject>> AddPermission_Base(AddEditPermissionRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            MTPermissionCenter_Permission Permission = new MTPermissionCenter_Permission()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                Description = requestBody.Description ?? "",
                Name = requestBody.Name,
                Key = requestBody.Key,
                Icon = requestBody.Icon,
                Routename = requestBody.Routename,
                PermissionType = requestBody.PermissionType ??"",
                MenuIds = requestBody.MenuIds,
                MenuParentId = requestBody.MenuParentId,
                OtherLangs = requestBody.OtherLangs ?? "",
                IsActive = true,
                
            };
            var result = await _PermissionRep.AddPermissionAsync(Permission);
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

                #endregion AddLog

                MTPermissionCenter_PermissionRole permissionRole = new MTPermissionCenter_PermissionRole() 
                { 
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    IsActive = true,
                    PermissionId = result.ID,
                    RoleId = 4,
                    OwnerOnly = false,
                };

                result = await _PermissionRoleRep.AddPermissionRolesAsync(new List<MTPermissionCenter_PermissionRole> { permissionRole });
                if (result.Status)
                {
                    #region AddLog

                    log = new Log()
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        LogTime = DateTime.Now.ToShamsi(),
                        ActionName = $"{this.ControllerContext.RouteData.Values["action"].ToString()}/AddPermissionRolesAsync",
                    };
                    await _logRep.AddLogAsync(log);

                    #endregion AddLog

                    await _PermissionInvalidationService.BumpRoleUsersVersionAsync(new List<long> {4});


                    return Ok(result);
                }
            }
            return BadRequest(result);
        }

        [HttpPut("EditPermission_Base")]
        public async Task<ActionResult<BitResultObject>> EditPermission_Base(AddEditPermissionRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var theRow = await _PermissionRep.GetPermissionByIdAsync(requestBody.ID);
            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            MTPermissionCenter_Permission Permission = new MTPermissionCenter_Permission()
            {
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ID = requestBody.ID,
                Description = requestBody.Description ?? "",
                Name = requestBody.Name,
                Key = requestBody.Key,
                IsActive = true,
                Icon = requestBody.Icon,
                Routename = requestBody.Routename,
                PermissionType = requestBody.PermissionType ?? "",
                MenuIds = requestBody.MenuIds,
                MenuParentId = requestBody.MenuParentId,
                OtherLangs = requestBody.OtherLangs ?? "",

            };
            result = await _PermissionRep.EditPermissionAsync(Permission);
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

                #endregion AddLog

                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("DeletePermission_Base")]
        public async Task<ActionResult<BitResultObject>> DeletePermission_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PermissionRep.RemovePermissionAsync(requestBody.ID);
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

                #endregion AddLog

                return Ok(result);
            }
            return BadRequest(result);
        }


        //[HttpPost("InitPermissions")]
        //[AllowAnonymous]
        //public async Task<ActionResult<BitResultObject>> InitPermissions([FromBody] List<ControllerActionInfo> requestBody)
        //{
        //    var result = new BitResultObject();
        //    int addCount = 0, noAddCount = 0;
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(requestBody);
        //    }

        //    try
        //    {
        //        foreach (var action in requestBody)
        //        {
        //            MTPermissionCenter_Permission Permission = new MTPermissionCenter_Permission()
        //            {
        //                CreateDate = DateTime.Now.ToShamsi(),
        //                UpdateDate = DateTime.Now.ToShamsi(),
        //                Description = "",
        //                Name = action.ActionName,
        //                Key = action.PermissionKey,
        //                Icon = "",
        //                Routename = "",
        //                PermissionType = "Action",
        //                OtherLangs = "",
        //                IsActive = true,

        //            };
        //            result = await _PermissionRep.AddPermissionAsync(Permission);

        //            if (result.Status)
        //            {
        //                var roleIds = new long[] { 1, 2, 3, 4 };
        //         foreach (var roleId in roleIds)
        //            {
        //                MTPermissionCenter_PermissionRole permissionRole = new MTPermissionCenter_PermissionRole()
        //                {
        //                    CreateDate = DateTime.Now.ToShamsi(),
        //                    UpdateDate = DateTime.Now.ToShamsi(),
        //                    IsActive = true,
        //                    PermissionId = Permission.ID,
        //                    RoleId = roleId,
        //                    OwnerOnly = roleId < 4,
        //                };

        //                result = await _PermissionRoleRep.AddPermissionRolesAsync(new List<MTPermissionCenter_PermissionRole>() { permissionRole });


        //                if (result.Status)
        //                {
        //                    addCount++;
        //                }

        //                else
        //                {
        //                    noAddCount++;
        //                }
        //            }
                




                  





        //            }


        //            if (result.Status)
        //        {
        //            #region AddLog

        //            Log log = new Log()
        //            {
        //                CreateDate = DateTime.Now.ToShamsi(),
        //                UpdateDate = DateTime.Now.ToShamsi(),
        //                LogTime = DateTime.Now.ToShamsi(),
        //                ActionName = $"{this.ControllerContext.RouteData.Values["action"].ToString()}/AddPermissionRolesAsync",
        //            };
        //            await _logRep.AddLogAsync(log);

        //                #endregion AddLog

        //                //await _PermissionInvalidationService.BumpRoleUsersVersionAsync(new List<long> { 4 });


        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.ErrorMessage = $"{ex.Message}\n{ex.InnerException?.Message}";
        //        result.Status = false;

        //        return BadRequest(result);
        //    }

        //    result.ErrorMessage = $"AddCount: {addCount} , NoAddCount: {noAddCount}";

        //    return Ok(result);
        //}


    }

    public class ControllerActionInfo
    {
        public string ActionName { get; set; } = "";
        public string PermissionKey { get; set; } = "";
        public bool HasAuth { get; set; }
        //public string AdminRoles { get; set; }

    }
}