using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.CustomerDashboard;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusAPI.Controllers
{
    [Route("CustomerDashboard")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class CustomerDashboardController : ControllerBase
    {
        private readonly ICustomerDashboardRep _customerDashboardRep;

        public CustomerDashboardController(ICustomerDashboardRep customerDashboardRep)
        {
            _customerDashboardRep = customerDashboardRep;
        }

        [HttpPost("GetCustomerDashboardReport_Base")]
        public async Task<ActionResult<RowResultObject<CustomerDashboardReport>>> GetCustomerDashboardReport_Base(GetCustomerDashboardReportRequestBody requestBody)
        {
            if (!ModelState.IsValid || requestBody.PersonId <= 0)
            {
                return BadRequest(requestBody);
            }

            var result = await _customerDashboardRep.GetCustomerDashboardReportAsync(requestBody.PersonId, requestBody.FromDate, requestBody.ToDate);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
