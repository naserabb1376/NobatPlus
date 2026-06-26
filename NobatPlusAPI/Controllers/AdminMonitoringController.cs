using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.AdminMonitoring;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("AdminMonitoring")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class AdminMonitoringController : ControllerBase
    {
        private readonly IAdminMonitoringRep _adminMonitoringRep;

        public AdminMonitoringController(IAdminMonitoringRep adminMonitoringRep)
        {
            _adminMonitoringRep = adminMonitoringRep;
        }

        [HttpPost("GetAdminMonitoringReport_Base")]
        public async Task<ActionResult<RowResultObject<AdminMonitoringReportVM>>> GetAdminMonitoringReport_Base(GetAdminMonitoringReportRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4) return Forbid();
            if (!ModelState.IsValid) return BadRequest(requestBody);

            var result = await _adminMonitoringRep.GetMonitoringReportAsync(requestBody.FromDate, requestBody.ToDate);
            return result.Status ? Ok(result) : BadRequest(result);
        }
    }
}
