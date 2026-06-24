using AutoMapper;
using Domains;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MTPermissionCenter.EFCore.Entities;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Permission;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusAPI.ViewModels;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Claims;
using System.Text;
using static NobatPlusAPI.Tools.ToolBox;
using static NobatPlusDATA.Tools.DbTools;
using MenuItem = NobatPlusAPI.Tools.ToolBox.MenuItem;

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
        // [RequireRole(4)]
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
        // [RequireRole(4)]
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
        // [RequireRole(4)]
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
        // [RequireRole(4)]
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
        // [RequireRole(4)]
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


#if DEBUG
        [HttpPost("InitPermissions")]
        [AllowAnonymous]
        public async Task<ActionResult<BitResultObject>> InitPermissions([FromBody] List<ControllerActionInfo> requestBody)
        {
            var result = new BitResultObject();
            int addCount = 0, noAddCount = 0;
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            try
            {
                foreach (var action in requestBody)
                {
                    MTPermissionCenter_Permission Permission = new MTPermissionCenter_Permission()
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        Description = "",
                        Name = action.ActionName,
                        Key = action.PermissionKey,
                        Icon = "",
                        Routename = "",
                        PermissionType = "Action",
                        OtherLangs = "",
                        IsActive = true,

                    };
                    result = await _PermissionRep.AddPermissionAsync(Permission);

                    if (result.Status)
                    {
                        var roleIds = new long[] { (long)BaseRole.Customer, (long)BaseRole.Stylist, (long)BaseRole.Salon, (long)BaseRole.Admin };
                        foreach (var roleId in roleIds)
                        {
                            MTPermissionCenter_PermissionRole permissionRole = new MTPermissionCenter_PermissionRole()
                            {
                                CreateDate = DateTime.Now.ToShamsi(),
                                UpdateDate = DateTime.Now.ToShamsi(),
                                IsActive = true,
                                PermissionId = Permission.ID,
                                RoleId = roleId,
                                OwnerOnly = false //roleId < (long)BaseRole.Admin,
                            };

                            result = await _PermissionRoleRep.AddPermissionRolesAsync(new List<MTPermissionCenter_PermissionRole>() { permissionRole });


                            if (result.Status)
                            {
                                addCount++;
                            }

                            else
                            {
                                noAddCount++;
                            }
                        }

                    }


                    if (result.Status)
                    {
                        #region AddLog

                        Log log = new Log()
                        {
                            CreateDate = DateTime.Now.ToShamsi(),
                            UpdateDate = DateTime.Now.ToShamsi(),
                            LogTime = DateTime.Now.ToShamsi(),
                            ActionName = $"{this.ControllerContext.RouteData.Values["action"].ToString()}/AddPermissionRolesAsync",
                        };
                        await _logRep.AddLogAsync(log);

                        #endregion AddLog

                        //await _PermissionInvalidationService.BumpRoleUsersVersionAsync(new List<long> { 4 });


                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"{ex.Message}\n{ex.InnerException?.Message}";
                result.Status = false;

                return BadRequest(result);
            }

            result.ErrorMessage = $"AddCount: {addCount} , NoAddCount: {noAddCount}";

            return Ok(result);
        }

        [HttpPost("InitMenus")]
        [AllowAnonymous]
        public async Task<ActionResult<BitResultObject>> InitMenus([FromBody] List<MenuItem> requestBody)
        {
            var result = new BitResultObject();
            int addCount = 0, noAddCount = 0;
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            try
            {
                var menuPerms = new List<MTPermissionCenter_Permission>();
                void ProcessItem(MenuItem item, long? parentId)
                {
                    var permission = new MTPermissionCenter_Permission
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        IsActive = true,

                        Key = new Random().Next(12000,30000).ToString(),
                        Name = item.label,
                        Icon = item.icon,
                        Routename = item.path,
                        PermissionType = "Menu",
                        MenuIds = null,
                        MenuParentId = parentId,
                        Description = item.Description,
                        OtherLangs = null
                    };

                    // تعیین RoleId از روی RoleName
                    if (Enum.TryParse<BaseRole>(item.RoleName,true, out var roleEnum))
                    {
                        permission.PermissionRoles.Add(new MTPermissionCenter_PermissionRole
                        {
                            CreateDate = DateTime.Now.ToShamsi(),
                            UpdateDate = DateTime.Now.ToShamsi(),
                            IsActive = true,

                            RoleId = (long)roleEnum,
                            OwnerOnly = false
                        });
                    }

                    menuPerms.Add(permission);

                   
                    // اگر children داشت، بازگشتی پردازش کن
                    if (item.children != null && item.children.Any())
                    {
                        foreach (var child in item.children)
                        {
                            ProcessItem(child, long.Parse(permission.Key)); // ParentId = Id همین آیتم
                        }
                    }
                }

                foreach (var item in requestBody)
                {
                    ProcessItem(item, null);
                }

                foreach (var menu in menuPerms)
                {
                    result = await _PermissionRep.AddPermissionAsync(menu);

                    if (result.Status)
                    {
                        addCount++;
                    }
                    else
                    {
                        noAddCount++;
                    }
                }

                result.ErrorMessage = $"AddCount: {addCount} , NoAddCount: {noAddCount}";

                return Ok(result);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"{ex.Message}\n{ex.InnerException?.Message}";
                result.Status = false;

                return BadRequest(result);
            }

        }

#endif


    }


}
