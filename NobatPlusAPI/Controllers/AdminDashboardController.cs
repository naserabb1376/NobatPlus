using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.AdminDashboard;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusAPI.Tools;

namespace NobatPlusAPI.Controllers
{
    [Route("AdminDashboard")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardRep _adminDashboardRep;

        public AdminDashboardController(IAdminDashboardRep adminDashboardRep)
        {
            _adminDashboardRep = adminDashboardRep;
        }

        [HttpPost("GetAdminDashboardReport_Base")]
        public async Task<ActionResult<RowResultObject<AdminDashboardReport>>> GetAdminDashboardReport_Base(GetAdminDashboardReportRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = await _adminDashboardRep.GetAdminDashboardReportAsync(requestBody.FromDate, requestBody.ToDate, requestBody.CityId, requestBody.RoleId);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
