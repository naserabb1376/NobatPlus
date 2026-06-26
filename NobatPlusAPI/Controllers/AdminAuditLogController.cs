using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.AdminAuditLog;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusAPI.Controllers
{
    [Route("AdminAuditLog")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class AdminAuditLogController : ControllerBase
    {
        private readonly IAdminAuditLogRep _adminAuditLogRep;

        public AdminAuditLogController(IAdminAuditLogRep adminAuditLogRep)
        {
            _adminAuditLogRep = adminAuditLogRep;
        }

        [HttpPost("GetAllAdminAuditLogs_Base")]
        public async Task<ActionResult<ListResultObject<AdminAuditLog>>> GetAllAdminAuditLogs_Base(GetAdminAuditLogListRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4) return Forbid();
            if (!ModelState.IsValid) return BadRequest(requestBody);

            var result = await _adminAuditLogRep.GetAllAdminAuditLogsAsync(
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.SearchText,
                requestBody.SortQuery,
                requestBody.ActorPersonId,
                requestBody.ActionName,
                requestBody.EntityName,
                requestBody.Succeeded,
                requestBody.FromDate,
                requestBody.ToDate);

            if (result.Status) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("GetAdminAuditLogById_Base")]
        public async Task<ActionResult<RowResultObject<AdminAuditLog>>> GetAdminAuditLogById_Base(GetRowRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4) return Forbid();
            if (!ModelState.IsValid) return BadRequest(requestBody);

            var result = await _adminAuditLogRep.GetAdminAuditLogByIdAsync(requestBody.ID);
            if (result.Status) return Ok(result);
            return BadRequest(result);
        }
    }
}
