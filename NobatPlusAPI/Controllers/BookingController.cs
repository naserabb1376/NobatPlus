using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.Booking;
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
    [Route("Booking")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class BookingController : ControllerBase
    {
        IBookingRep _BookingRep;
        ILogRep _logRep;

        public BookingController(IBookingRep BookingRep,ILogRep logRep)
        {
           _BookingRep = BookingRep;
            _logRep = logRep;
        }

        [HttpPost("GetAllBookings_Base")]
        public async Task<ActionResult<ListResultObject<Booking>>> GetAllBookings_Base(GetBookingListRequestBody requestBody)
        {
            var result = new ListResultObject<Booking>();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            if (requestBody.ServiceId > 0)
            {
                 result = await _BookingRep.GetBookingsOfServiceAsync(requestBody.ServiceId,requestBody.CancelState, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
                if (result.Status)
                {
                    return Ok(result);
                }
            }

            else
            {
                result = await _BookingRep.GetAllBookingsAsync(requestBody.CancelState, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText,requestBody.SortQuery);
                if (result.Status)
                {
                    return Ok(result);
                }
            }
            
            return BadRequest(result);
        }


        [HttpPost("GetBookingById_Base")]
        public async Task<ActionResult<RowResultObject<Booking>>> GetBookingById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingRep.GetBookingByIdAsync(requestBody.ID);
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistBooking_Base")]
        public async Task<ActionResult<BitResultObject>> ExistBooking_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingRep.ExistBookingAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddBooking_Base")]
        public async Task<ActionResult<BitResultObject>> AddBooking_Base(AddEditBookingRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            Booking Booking = new Booking()
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingDate = requestBody.BookingDate,
                BookingTime = requestBody.BookingTime,
                CancelReason = requestBody.CancellReason??"",
                CustomerID = requestBody.CustomerID,
                IsCancelled = requestBody.IsCancelled,
                Status = requestBody.Status,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description,
            };
            var result = await _BookingRep.AddBookingAsync(Booking);
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

        [HttpPut("EditBooking_Base")]
        public async Task<ActionResult<BitResultObject>> EditBooking_Base(AddEditBookingRequestBody requestBody)
        {
            var result = new BitResultObject();
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var theRow = await _BookingRep.GetBookingByIdAsync(requestBody.ID);

            if (!theRow.Status)
            {
                result.Status = theRow.Status;
                result.ErrorMessage = theRow.ErrorMessage;
            }

            Booking Booking = new Booking()
            {
                ID = requestBody.ID,
                CreateDate = theRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                BookingDate = requestBody.BookingDate,
                BookingTime = requestBody.BookingTime,
                CancelReason = requestBody.CancellReason ?? "",
                CustomerID = requestBody.CustomerID,
                IsCancelled = requestBody.IsCancelled,
                Status = requestBody.Status,
                StylistID = requestBody.StylistID,
                Description = requestBody.Description,
            };
             result = await _BookingRep.EditBookingAsync(Booking);
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

        [HttpDelete("DeleteBooking_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteBooking_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _BookingRep.RemoveBookingAsync(requestBody.ID);
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
