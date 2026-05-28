using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.PaymentBooking;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("PaymentBooking")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class PaymentBookingController : ControllerBase
    {
        private readonly IPaymentBookingRep _paymentBookingRep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public PaymentBookingController(IPaymentBookingRep paymentBookingRep, ILogRep logRep, IMapper mapper)
        {
            _paymentBookingRep = paymentBookingRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllPaymentBookings_Base")]
        public async Task<ActionResult<ListResultObject<PaymentBookingVM>>> GetAllPaymentBookings_Base(GetPaymentBookingListRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _paymentBookingRep.GetAllPaymentBookingsAsync(requestBody.PaymentID, requestBody.BookingID, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<ListResultObject<PaymentBookingVM>>(result));
        }

        [HttpPost("GetPaymentBookingById_Base")]
        public async Task<ActionResult<RowResultObject<PaymentBookingVM>>> GetPaymentBookingById_Base(PaymentBookingRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _paymentBookingRep.GetPaymentBookingByIdAsync(requestBody.PaymentID, requestBody.BookingID);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<RowResultObject<PaymentBookingVM>>(result));
        }

        [HttpPost("ExistPaymentBooking_Base")]
        public async Task<ActionResult<BitResultObject>> ExistPaymentBooking_Base(PaymentBookingRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _paymentBookingRep.ExistPaymentBookingAsync(requestBody.PaymentID, requestBody.BookingID);
            if (string.IsNullOrEmpty(result.ErrorMessage)) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("AddPaymentBookings_Base")]
        public async Task<ActionResult<BitResultObject>> AddPaymentBookings_Base(List<PaymentBookingRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new PaymentBooking { PaymentID = x.PaymentID, BookingID = x.BookingID }).ToList();
            var result = await _paymentBookingRep.AddPaymentBookingsAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpPut("EditPaymentBookings_Base")]
        public async Task<ActionResult<BitResultObject>> EditPaymentBookings_Base(List<PaymentBookingRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new PaymentBooking { PaymentID = x.PaymentID, BookingID = x.BookingID }).ToList();
            var result = await _paymentBookingRep.EditPaymentBookingsAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpDelete("DeletePaymentBookings_Base")]
        public async Task<ActionResult<BitResultObject>> DeletePaymentBookings_Base(List<PaymentBookingRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var ids = requestBodies.Select(x => (x.PaymentID, x.BookingID)).ToList();
            var result = await _paymentBookingRep.RemovePaymentBookingsAsync(ids);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        private async Task AddLogAsync()
        {
            await _logRep.AddLogAsync(new Log
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LogTime = DateTime.Now.ToShamsi(),
                ActionName = ControllerContext.RouteData.Values["action"].ToString()
            });
        }
    }
}
