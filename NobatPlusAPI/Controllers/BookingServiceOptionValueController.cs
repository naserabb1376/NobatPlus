using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.BookingServiceOptionValue;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("BookingServiceOptionValue")]
    [ApiController]
    [Authorize]
    [RequireRole(4)]
    [Produces("application/json")]
    public class BookingServiceOptionValueController : ControllerBase
    {
        private readonly IBookingServiceOptionValueRep _rep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public BookingServiceOptionValueController(IBookingServiceOptionValueRep rep, ILogRep logRep, IMapper mapper)
        {
            _rep = rep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllBookingServiceOptionValues_Base")]
        public async Task<ActionResult<ListResultObject<BookingServiceOptionValueVM>>> GetAllBookingServiceOptionValues_Base(GetBookingServiceOptionValueListRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.GetAllBookingServiceOptionValuesAsync(requestBody.BookingID, requestBody.ServiceManagementID, requestBody.ServiceOptionValueID, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<ListResultObject<BookingServiceOptionValueVM>>(result));
        }

        [HttpPost("GetBookingServiceOptionValueById_Base")]
        public async Task<ActionResult<RowResultObject<BookingServiceOptionValueVM>>> GetBookingServiceOptionValueById_Base(BookingServiceOptionValueRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.GetBookingServiceOptionValueByIdAsync(requestBody.BookingID, requestBody.ServiceManagementID, requestBody.ServiceOptionValueID);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<RowResultObject<BookingServiceOptionValueVM>>(result));
        }

        [HttpPost("ExistBookingServiceOptionValue_Base")]
        public async Task<ActionResult<BitResultObject>> ExistBookingServiceOptionValue_Base(BookingServiceOptionValueRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.ExistBookingServiceOptionValueAsync(requestBody.BookingID, requestBody.ServiceManagementID, requestBody.ServiceOptionValueID);
            if (string.IsNullOrEmpty(result.ErrorMessage)) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("AddBookingServiceOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> AddBookingServiceOptionValues_Base(List<BookingServiceOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new BookingServiceOptionValue { BookingID = x.BookingID, ServiceManagementID = x.ServiceManagementID, ServiceOptionValueID = x.ServiceOptionValueID }).ToList();
            var result = await _rep.AddBookingServiceOptionValuesAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpPut("EditBookingServiceOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> EditBookingServiceOptionValues_Base(List<BookingServiceOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new BookingServiceOptionValue { BookingID = x.BookingID, ServiceManagementID = x.ServiceManagementID, ServiceOptionValueID = x.ServiceOptionValueID }).ToList();
            var result = await _rep.EditBookingServiceOptionValuesAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpDelete("DeleteBookingServiceOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteBookingServiceOptionValues_Base(List<BookingServiceOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var ids = requestBodies.Select(x => (x.BookingID, x.ServiceManagementID, x.ServiceOptionValueID)).ToList();
            var result = await _rep.RemoveBookingServiceOptionValuesAsync(ids);
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
