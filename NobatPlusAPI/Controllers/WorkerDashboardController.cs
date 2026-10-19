using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.WorkerDashboard;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusAPI.Controllers
{
    [Route("WorkerDashboard")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class WorkerDashboardController : ControllerBase
    {
        private readonly IWorkerDashboardRep _workerDashboardRep;

        public WorkerDashboardController(IWorkerDashboardRep workerDashboardRep)
        {
            _workerDashboardRep = workerDashboardRep;
        }

        [HttpPost("GetWorkerDashboardReport_Base")]
        public async Task<ActionResult<RowResultObject<WorkerDashboardReport>>> GetWorkerDashboardReport_Base(GetWorkerDashboardReportRequestBody requestBody)
        {
            if (!ModelState.IsValid || requestBody.WorkerId <= 0)
            {
                return BadRequest(requestBody);
            }

            var result = await _workerDashboardRep.GetWorkerDashboardReportAsync(requestBody.WorkerId, requestBody.FromDate, requestBody.ToDate);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("GetWorkshopDashboardReport_Base")]
        public async Task<ActionResult<RowResultObject<WorkshopDashboardReport>>> GetWorkshopDashboardReport_Base(GetWorkerDashboardReportRequestBody requestBody)
        {
            if (!ModelState.IsValid || requestBody.WorkerId <= 0)
            {
                return BadRequest(requestBody);
            }

            var result = await _workerDashboardRep.GetWorkshopDashboardReportAsync(requestBody.WorkerId, requestBody.FromDate, requestBody.ToDate);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
