using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.StylistDashboard;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusAPI.Controllers
{
    [Route("StylistDashboard")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class StylistDashboardController : ControllerBase
    {
        private readonly IStylistDashboardRep _stylistDashboardRep;

        public StylistDashboardController(IStylistDashboardRep stylistDashboardRep)
        {
            _stylistDashboardRep = stylistDashboardRep;
        }

        [HttpPost("GetStylistDashboardReport_Base")]
        public async Task<ActionResult<RowResultObject<StylistDashboardReport>>> GetStylistDashboardReport_Base(GetStylistDashboardReportRequestBody requestBody)
        {
            if (!ModelState.IsValid || requestBody.StylistId <= 0)
            {
                return BadRequest(requestBody);
            }

            var result = await _stylistDashboardRep.GetStylistDashboardReportAsync(requestBody.StylistId, requestBody.FromDate, requestBody.ToDate);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("GetSalonDashboardReport_Base")]
        public async Task<ActionResult<RowResultObject<SalonDashboardReport>>> GetSalonDashboardReport_Base(GetStylistDashboardReportRequestBody requestBody)
        {
            if (!ModelState.IsValid || requestBody.StylistId <= 0)
            {
                return BadRequest(requestBody);
            }

            var result = await _stylistDashboardRep.GetSalonDashboardReportAsync(requestBody.StylistId, requestBody.FromDate, requestBody.ToDate);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
