using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.BookingTag;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("BookingTag"), ApiController, Authorize, Produces("application/json")]
    public class BookingTagController : ControllerBase
    {
        private readonly IBookingTagRep _rep;
        private readonly IStylistRep _stylistRep;
        private readonly IMapper _mapper;

        public BookingTagController(IBookingTagRep rep, IStylistRep stylistRep, IMapper mapper)
        { _rep = rep; _stylistRep = stylistRep; _mapper = mapper; }

        [HttpPost("GetAllBookingTags_Base")]
        public async Task<ActionResult<ListResultObject<BookingTagVM>>> GetAllBookingTags_Base(GetBookingTagListRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            if (!await CanManageStylistAsync(request.StylistID)) return Forbid();
            var result = await _rep.GetAllBookingTagsAsync(request.StylistID, request.IsActive, request.PageIndex, request.PageSize, request.SearchText, request.SortQuery);
            return result.Status ? Ok(_mapper.Map<ListResultObject<BookingTagVM>>(result)) : BadRequest(result);
        }

        [HttpPost("GetBookingTagById_Base")]
        public async Task<ActionResult<RowResultObject<BookingTagVM>>> GetBookingTagById_Base(GetRowRequestBody request)
        {
            var result = await _rep.GetBookingTagByIdAsync(request.ID);
            if (!result.Status || result.Result == null) return BadRequest(result);
            if (!await CanManageStylistAsync(result.Result.StylistID)) return Forbid();
            return Ok(_mapper.Map<RowResultObject<BookingTagVM>>(result));
        }

        [HttpPost("AddBookingTag_Base")]
        public async Task<ActionResult<BitResultObject>> AddBookingTag_Base(AddEditBookingTagRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            if (!await CanManageStylistAsync(request.StylistID)) return Forbid();
            var result = await _rep.AddBookingTagAsync(Build(request, DateTime.Now.ToShamsi()));
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPut("EditBookingTag_Base")]
        public async Task<ActionResult<BitResultObject>> EditBookingTag_Base(AddEditBookingTagRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            var old = await _rep.GetBookingTagByIdAsync(request.ID);
            if (!old.Status || old.Result == null) return BadRequest(old);
            if (!await CanManageStylistAsync(old.Result.StylistID) || old.Result.StylistID != request.StylistID) return Forbid();
            var result = await _rep.EditBookingTagAsync(Build(request, old.Result.CreateDate));
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("DeleteBookingTag_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteBookingTag_Base(GetRowRequestBody request)
        {
            var old = await _rep.GetBookingTagByIdAsync(request.ID);
            if (!old.Status || old.Result == null) return BadRequest(old);
            if (!await CanManageStylistAsync(old.Result.StylistID)) return Forbid();
            var result = await _rep.RemoveBookingTagAsync(request.ID);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        private static BookingTag Build(AddEditBookingTagRequestBody x, DateTime? createDate) => new()
        { ID = x.ID, StylistID = x.StylistID, Title = x.Title, Color = x.Color, SortOrder = x.SortOrder, IsActive = x.IsActive, Description = x.Description, CreateDate = createDate, UpdateDate = DateTime.Now.ToShamsi() };

        private async Task<bool> CanManageStylistAsync(long stylistId)
        {
            if (User.GetCurrentRoleId() == (long)DbTools.BaseRole.Admin) return true;
            var current = User.GetCurrentProfileId() > 0 ? User.GetCurrentProfileId() : User.GetCurrentStylistId();
            if (current <= 0) return false;
            if (stylistId == current) return true;
            if (User.GetCurrentRoleId() != (long)DbTools.BaseRole.Salon) return false;
            var target = await _stylistRep.GetStylistByIdAsync(stylistId);
            return target.Status && target.Result?.StylistParentID == current;
        }
    }
}
