using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.WorkerServiceFollowUpSetting;
using NobatPlusAPI.ViewModels;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusAPI.Controllers
{
    [Route("WorkerServiceFollowUpSetting")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class WorkerServiceFollowUpSettingController : ControllerBase
    {
        private readonly IWorkerServiceFollowUpSettingRep _followUpSettingRep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public WorkerServiceFollowUpSettingController(IWorkerServiceFollowUpSettingRep followUpSettingRep, ILogRep logRep, IMapper mapper)
        {
            _followUpSettingRep = followUpSettingRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllWorkerServiceFollowUpSettings_Base")]
        public async Task<ActionResult<ListResultObject<WorkerServiceFollowUpSettingVM>>> GetAllWorkerServiceFollowUpSettings_Base(GetWorkerServiceFollowUpSettingListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _followUpSettingRep.GetAllWorkerServiceFollowUpSettingsAsync(
                requestBody.WorkerID,
                requestBody.ServiceManagementID,
                requestBody.IsActive,
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.SearchText,
                requestBody.SortQuery);

            if (result.Status)
                return Ok(_mapper.Map<ListResultObject<WorkerServiceFollowUpSettingVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("GetWorkerServiceFollowUpSettingById_Base")]
        public async Task<ActionResult<RowResultObject<WorkerServiceFollowUpSettingVM>>> GetWorkerServiceFollowUpSettingById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _followUpSettingRep.GetWorkerServiceFollowUpSettingByIdAsync(requestBody.ID);
            if (result.Status)
                return Ok(_mapper.Map<RowResultObject<WorkerServiceFollowUpSettingVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("ExistWorkerServiceFollowUpSetting_Base")]
        public async Task<ActionResult<BitResultObject>> ExistWorkerServiceFollowUpSetting_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _followUpSettingRep.ExistWorkerServiceFollowUpSettingAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("AddWorkerServiceFollowUpSetting_Base")]
        public async Task<ActionResult<BitResultObject>> AddWorkerServiceFollowUpSetting_Base(AddEditWorkerServiceFollowUpSettingRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var row = BuildSetting(requestBody, DateTime.Now.ToShamsi());
            var result = await _followUpSettingRep.AddWorkerServiceFollowUpSettingAsync(row);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("EditWorkerServiceFollowUpSetting_Base")]
        public async Task<ActionResult<BitResultObject>> EditWorkerServiceFollowUpSetting_Base(AddEditWorkerServiceFollowUpSettingRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var oldRow = await _followUpSettingRep.GetWorkerServiceFollowUpSettingByIdAsync(requestBody.ID);
            if (!oldRow.Status || oldRow.Result == null)
                return BadRequest(oldRow);

            var row = BuildSetting(requestBody, oldRow.Result.CreateDate);
            var result = await _followUpSettingRep.EditWorkerServiceFollowUpSettingAsync(row);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteWorkerServiceFollowUpSetting_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteWorkerServiceFollowUpSetting_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _followUpSettingRep.RemoveWorkerServiceFollowUpSettingAsync(requestBody.ID);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        private static WorkerServiceFollowUpSetting BuildSetting(AddEditWorkerServiceFollowUpSettingRequestBody requestBody, DateTime? createDate)
        {
            return new WorkerServiceFollowUpSetting
            {
                ID = requestBody.ID,
                CreateDate = createDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                WorkerID = requestBody.WorkerID,
                ServiceManagementID = requestBody.ServiceManagementID,
                WorkerServicePriceVariantID = requestBody.WorkerServicePriceVariantID > 0 ? requestBody.WorkerServicePriceVariantID : null,
                RepairEnabled = requestBody.RepairEnabled,
                RepairAfterDays = requestBody.RepairAfterDays,
                RepairReminderEnabled = requestBody.RepairReminderEnabled,
                RepairReminderBeforeDays = requestBody.RepairReminderBeforeDays,
                RepairReminderMessageSettingKey = requestBody.RepairReminderMessageSettingKey,
                AfterCareEnabled = requestBody.AfterCareEnabled,
                AfterCareDelayMinutes = requestBody.AfterCareDelayMinutes,
                AfterCareMessageSettingKey = requestBody.AfterCareMessageSettingKey,
                AfterCareInstructions = requestBody.AfterCareInstructions,
                IsActive = requestBody.IsActive,
                Description = requestBody.Description
            };
        }

        private async Task AddLogAsync()
        {
            await _logRep.AddLogAsync(new Log
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                LogTime = DateTime.Now.ToShamsi(),
                ActionName = ControllerContext.RouteData.Values["action"]?.ToString()
            });
        }
    }
}
