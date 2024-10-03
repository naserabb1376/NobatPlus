using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.BookingService;
using NobatPlusAPI.Models.Public;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NobatPlusAPI.Controllers
{
    [Route("BookingService")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class BookingServiceController : ControllerBase
    {
        IBookingServiceRep _BookingServiceRep;
        ILogRep _logRep;

        public BookingServiceController(IBookingServiceRep BookingServiceRep,ILogRep logRep)
        {
           _BookingServiceRep = BookingServiceRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllBookingServices_Base")]
        public async Task<ActionResult<ListResultObject<BookingService>>> GetAllBookingServices_Base(GetBookingServiceListRequestBody requestBody)
        {
            var result = new ListResultObject<BookingService>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            result = await _BookingServiceRep.GetAllBookingServicesAsync(requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
            if (result.Status)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpPost("GetBookingServiceById_Base")]
        public async Task<ActionResult<ListResultObject<BookingService>>> GetBookingServiceById_Base(GetBookingServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingServiceRep.GetBookingServiceByIdAsync(requestBody.BookingID,requestBody.ServiceID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistBookingService_Base")]
        public async Task<ActionResult<BitResultObject>> ExistBookingService_Base(GetBookingServiceRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingServiceRep.ExistBookingServiceAsync(requestBody.BookingID, requestBody.ServiceID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddBookingServices_Base")]
        public async Task<ActionResult<BitResultObject>> AddBookingServices_Base(List<GetBookingServiceRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodies);
            }

            var bookingServices = requestBodies.Select(requestBody => new BookingService
            {
                BookingID = requestBody.BookingID,
                ServiceManagementID = requestBody.ServiceID,
            }).ToList();

            var result = await _BookingServiceRep.AddBookingServicesAsync(bookingServices);

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

                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpPut("EditBookingServices_Base")]
        public async Task<ActionResult<BitResultObject>> EditBookingServices_Base(List<GetBookingServiceRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodies);
            }

            var bookingServices = requestBodies.Select(requestBody => new BookingService
            {
                BookingID = requestBody.BookingID,
                ServiceManagementID = requestBody.ServiceID,
            }).ToList();

            var result = await _BookingServiceRep.EditBookingServicesAsync(bookingServices);

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

                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpDelete("DeleteBookingServices_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteBookingServices_Base(List<GetBookingServiceRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBodies);
            }

            var bookingServiceIds = requestBodies.Select(requestBody =>
                (requestBody.BookingID, requestBody.ServiceID)).ToList();

            var result = await _BookingServiceRep.RemoveBookingServicesAsync(bookingServiceIds);

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

                return Ok(result);
            }

            return BadRequest(result);
        }

    }
}
