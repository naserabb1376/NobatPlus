using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.StylistScheduleBlock;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("StylistScheduleBlock"), ApiController, Authorize, Produces("application/json")]
    public class StylistScheduleBlockController : ControllerBase
    {
        private readonly IStylistScheduleBlockRep _rep;
        private readonly IStylistRep _stylistRep;
        private readonly IMapper _mapper;

        public StylistScheduleBlockController(IStylistScheduleBlockRep rep, IStylistRep stylistRep, IMapper mapper)
        { _rep = rep; _stylistRep = stylistRep; _mapper = mapper; }

        [HttpPost("GetPublicScheduleSlots"), AllowAnonymous]
        public async Task<ActionResult<ListResultObject<StylistScheduleBlockVM>>> GetPublicScheduleSlots(GetStylistScheduleBlockListRequestBody request)
        {
            if (!ModelState.IsValid || request.FromDate == null || request.ToDate == null || request.ToDate < request.FromDate)
                return BadRequest("آرایشگر و بازه تاریخ معتبر الزامی است.");
            if ((request.ToDate.Value - request.FromDate.Value).TotalDays > 31) return BadRequest("بازه دریافت زمان‌ها نمی‌تواند بیشتر از ۳۱ روز باشد.");
            var rangeEnd = request.ToDate.Value.TimeOfDay == TimeSpan.Zero ? request.ToDate.Value.Date.AddDays(1).AddTicks(-1) : request.ToDate.Value;
            var result = await _rep.GetAllStylistScheduleBlocksAsync(request.StylistID, request.FromDate, rangeEnd, request.ServiceManagementID, request.BookingTagID, "available", 0, request.DiscountID, request.PageIndex, request.PageSize <= 0 ? 500 : request.PageSize, request.SearchText, request.SortQuery);
            return result.Status ? Ok(_mapper.Map<ListResultObject<StylistScheduleBlockVM>>(result)) : BadRequest(result);
        }

        [HttpPost("GetAllStylistScheduleBlocks_Base")]
        public async Task<ActionResult<ListResultObject<StylistScheduleBlockVM>>> GetAllStylistScheduleBlocks_Base(GetStylistScheduleBlockListRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            if (!await CanManageStylistAsync(request.StylistID)) return Forbid();
            var result = await _rep.GetAllStylistScheduleBlocksAsync(request.StylistID, request.FromDate, request.ToDate, request.ServiceManagementID, request.BookingTagID, request.Status, request.CustomerID, request.DiscountID, request.PageIndex, request.PageSize, request.SearchText, request.SortQuery);
            return result.Status ? Ok(_mapper.Map<ListResultObject<StylistScheduleBlockVM>>(result)) : BadRequest(result);
        }

        [HttpPost("GetStylistScheduleBlockById_Base")]
        public async Task<ActionResult<RowResultObject<StylistScheduleBlockVM>>> GetStylistScheduleBlockById_Base(GetRowRequestBody request)
        {
            var result = await _rep.GetStylistScheduleBlockByIdAsync(request.ID);
            if (!result.Status || result.Result == null) return BadRequest(result);
            if (!await CanManageStylistAsync(result.Result.StylistID)) return Forbid();
            return Ok(_mapper.Map<RowResultObject<StylistScheduleBlockVM>>(result));
        }

        [HttpPost("AddStylistScheduleBlocks_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylistScheduleBlocks_Base(AddStylistScheduleBlocksRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            foreach (var stylistId in request.Blocks.Select(x => x.StylistID).Distinct()) if (!await CanManageStylistAsync(stylistId)) return Forbid();
            var now = DateTime.Now.ToShamsi();
            var result = await _rep.AddStylistScheduleBlocksAsync(request.Blocks.Select(x => Build(x, now)).ToList());
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPut("EditStylistScheduleBlock_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylistScheduleBlock_Base(StylistScheduleBlockItemRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            var old = await _rep.GetStylistScheduleBlockByIdAsync(request.ID);
            if (!old.Status || old.Result == null) return BadRequest(old);
            if (!await CanManageStylistAsync(old.Result.StylistID) || old.Result.StylistID != request.StylistID) return Forbid();
            StylistScheduleBlock row;
            try { row = Build(request, old.Result.CreateDate); }
            catch (FormatException) { return BadRequest("RowVersion معتبر نیست."); }
            var result = await _rep.EditStylistScheduleBlockAsync(row);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("DeleteStylistScheduleBlock_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistScheduleBlock_Base(GetRowRequestBody request)
        {
            var old = await _rep.GetStylistScheduleBlockByIdAsync(request.ID);
            if (!old.Status || old.Result == null) return BadRequest(old);
            if (!await CanManageStylistAsync(old.Result.StylistID)) return Forbid();
            var result = await _rep.RemoveStylistScheduleBlockAsync(request.ID);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        private static StylistScheduleBlock Build(StylistScheduleBlockItemRequestBody x, DateTime? createDate) => new()
        {
            ID = x.ID, StylistID = x.StylistID, StartDateTime = x.StartDateTime, EndDateTime = x.EndDateTime,
            ServiceManagementID = x.ServiceManagementID, StylistServicePriceVariantID = x.StylistServicePriceVariantID,
            BookingTagID = x.BookingTagID, Title = x.Title, PriceOverride = x.PriceOverride,
            DepositPercentOverride = x.DepositPercentOverride, IsBookable = x.IsBookable, IsActive = x.IsActive,
            Description = x.Description, CreateDate = createDate, UpdateDate = DateTime.Now.ToShamsi(),
            RowVersion = string.IsNullOrWhiteSpace(x.RowVersion) ? Array.Empty<byte>() : Convert.FromBase64String(x.RowVersion)
        };

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
