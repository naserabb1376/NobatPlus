using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.AdminActionCenter;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("AdminActionCenter")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class AdminActionCenterController : ControllerBase
    {
        private readonly IAdminActionCenterRep _adminActionCenterRep;

        public AdminActionCenterController(IAdminActionCenterRep adminActionCenterRep)
        {
            _adminActionCenterRep = adminActionCenterRep;
        }

        [HttpPost("GetAdminActionCenter_Base")]
        public async Task<ActionResult<RowResultObject<AdminActionCenterVM>>> GetAdminActionCenter_Base(GetAdminActionCenterRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4) return Forbid();
            if (!ModelState.IsValid) return BadRequest(requestBody);

            var result = await _adminActionCenterRep.GetActionCenterAsync(
                requestBody.Type,
                requestBody.Severity,
                requestBody.MaxItemsPerType);

            return result.Status ? Ok(result) : BadRequest(result);
        }
    }
}
