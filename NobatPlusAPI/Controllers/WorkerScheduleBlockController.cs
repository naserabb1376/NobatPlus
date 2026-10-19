using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.WorkerScheduleBlock;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("WorkerScheduleBlock"), ApiController, Authorize, Produces("application/json")]
    public class WorkerScheduleBlockController : ControllerBase
    {
        private readonly IWorkerScheduleBlockRep _rep;
        private readonly IWorkerRep _workerRep;
        private readonly IMapper _mapper;

        public WorkerScheduleBlockController(IWorkerScheduleBlockRep rep, IWorkerRep workerRep, IMapper mapper)
        { _rep = rep; _workerRep = workerRep; _mapper = mapper; }

        [HttpPost("GetPublicScheduleSlots"), AllowAnonymous]
        public async Task<ActionResult<ListResultObject<WorkerScheduleBlockVM>>> GetPublicScheduleSlots(GetWorkerScheduleBlockListRequestBody request)
        {
            if (!ModelState.IsValid || request.FromDate == null || request.ToDate == null || request.ToDate < request.FromDate)
                return BadRequest("خدمات دهنده و بازه تاریخ معتبر الزامی است.");
            if ((request.ToDate.Value - request.FromDate.Value).TotalDays > 31) return BadRequest("بازه دریافت زمان‌ها نمی‌تواند بیشتر از ۳۱ روز باشد.");
            var rangeEnd = request.ToDate.Value.TimeOfDay == TimeSpan.Zero ? request.ToDate.Value.Date.AddDays(1).AddTicks(-1) : request.ToDate.Value;
            var result = await _rep.GetAllWorkerScheduleBlocksAsync(request.WorkerID, request.FromDate, rangeEnd, request.ServiceManagementID, request.BookingTagID, "available", 0, request.DiscountID, request.PageIndex, request.PageSize <= 0 ? 500 : request.PageSize, request.SearchText, request.SortQuery);
            return result.Status ? Ok(_mapper.Map<ListResultObject<WorkerScheduleBlockVM>>(result)) : BadRequest(result);
        }

        [HttpPost("GetAllWorkerScheduleBlocks_Base")]
        public async Task<ActionResult<ListResultObject<WorkerScheduleBlockVM>>> GetAllWorkerScheduleBlocks_Base(GetWorkerScheduleBlockListRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            if (!await CanManageWorkerAsync(request.WorkerID)) return Forbid();
            var result = await _rep.GetAllWorkerScheduleBlocksAsync(request.WorkerID, request.FromDate, request.ToDate, request.ServiceManagementID, request.BookingTagID, request.Status, request.CustomerID, request.DiscountID, request.PageIndex, request.PageSize, request.SearchText, request.SortQuery);
            return result.Status ? Ok(_mapper.Map<ListResultObject<WorkerScheduleBlockVM>>(result)) : BadRequest(result);
        }

        [HttpPost("GetWorkerScheduleBlockById_Base")]
        public async Task<ActionResult<RowResultObject<WorkerScheduleBlockVM>>> GetWorkerScheduleBlockById_Base(GetRowRequestBody request)
        {
            var result = await _rep.GetWorkerScheduleBlockByIdAsync(request.ID);
            if (!result.Status || result.Result == null) return BadRequest(result);
            if (!await CanManageWorkerAsync(result.Result.WorkerID)) return Forbid();
            return Ok(_mapper.Map<RowResultObject<WorkerScheduleBlockVM>>(result));
        }

        [HttpPost("AddWorkerScheduleBlocks_Base")]
        public async Task<ActionResult<BitResultObject>> AddWorkerScheduleBlocks_Base(AddWorkerScheduleBlocksRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            foreach (var workerId in request.Blocks.Select(x => x.WorkerID).Distinct()) if (!await CanManageWorkerAsync(workerId)) return Forbid();
            var now = DateTime.Now.ToShamsi();
            var result = await _rep.AddWorkerScheduleBlocksAsync(request.Blocks.Select(x => Build(x, now)).ToList());
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPut("EditWorkerScheduleBlock_Base")]
        public async Task<ActionResult<BitResultObject>> EditWorkerScheduleBlock_Base(WorkerScheduleBlockItemRequestBody request)
        {
            if (!ModelState.IsValid) return BadRequest(request);
            var old = await _rep.GetWorkerScheduleBlockByIdAsync(request.ID);
            if (!old.Status || old.Result == null) return BadRequest(old);
            if (!await CanManageWorkerAsync(old.Result.WorkerID) || old.Result.WorkerID != request.WorkerID) return Forbid();
            WorkerScheduleBlock row;
            try { row = Build(request, old.Result.CreateDate); }
            catch (FormatException) { return BadRequest("RowVersion معتبر نیست."); }
            var result = await _rep.EditWorkerScheduleBlockAsync(row);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("DeleteWorkerScheduleBlock_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteWorkerScheduleBlock_Base(GetRowRequestBody request)
        {
            var old = await _rep.GetWorkerScheduleBlockByIdAsync(request.ID);
            if (!old.Status || old.Result == null) return BadRequest(old);
            if (!await CanManageWorkerAsync(old.Result.WorkerID)) return Forbid();
            var result = await _rep.RemoveWorkerScheduleBlockAsync(request.ID);
            return result.Status ? Ok(result) : BadRequest(result);
        }

        private static WorkerScheduleBlock Build(WorkerScheduleBlockItemRequestBody x, DateTime? createDate) => new()
        {
            ID = x.ID, WorkerID = x.WorkerID, StartDateTime = x.StartDateTime, EndDateTime = x.EndDateTime,
            ServiceManagementID = x.ServiceManagementID, WorkerServicePriceVariantID = x.WorkerServicePriceVariantID,
            BookingTagID = x.BookingTagID, Title = x.Title, PriceOverride = x.PriceOverride,
            DepositPercentOverride = x.DepositPercentOverride, IsBookable = x.IsBookable, IsActive = x.IsActive,
            Description = x.Description, CreateDate = createDate, UpdateDate = DateTime.Now.ToShamsi(),
            RowVersion = string.IsNullOrWhiteSpace(x.RowVersion) ? Array.Empty<byte>() : Convert.FromBase64String(x.RowVersion)
        };

        private async Task<bool> CanManageWorkerAsync(long workerId)
        {
            if (User.GetCurrentRoleId() == (long)DbTools.BaseRole.Admin) return true;
            var current = User.GetCurrentProfileId() > 0 ? User.GetCurrentProfileId() : User.GetCurrentWorkerId();
            if (current <= 0) return false;
            if (workerId == current) return true;
            if (User.GetCurrentRoleId() != (long)DbTools.BaseRole.WorkShop) return false;
            var target = await _workerRep.GetWorkerByIdAsync(workerId);
            return target.Status && target.Result?.WorkerParentID == current;
        }
    }
}
