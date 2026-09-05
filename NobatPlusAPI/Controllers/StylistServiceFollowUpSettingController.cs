using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.StylistServiceFollowUpSetting;
using NobatPlusAPI.ViewModels;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusAPI.Controllers
{
    [Route("StylistServiceFollowUpSetting")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class StylistServiceFollowUpSettingController : ControllerBase
    {
        private readonly IStylistServiceFollowUpSettingRep _followUpSettingRep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public StylistServiceFollowUpSettingController(IStylistServiceFollowUpSettingRep followUpSettingRep, ILogRep logRep, IMapper mapper)
        {
            _followUpSettingRep = followUpSettingRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllStylistServiceFollowUpSettings_Base")]
        public async Task<ActionResult<ListResultObject<StylistServiceFollowUpSettingVM>>> GetAllStylistServiceFollowUpSettings_Base(GetStylistServiceFollowUpSettingListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _followUpSettingRep.GetAllStylistServiceFollowUpSettingsAsync(
                requestBody.StylistID,
                requestBody.ServiceManagementID,
                requestBody.IsActive,
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.SearchText,
                requestBody.SortQuery);

            if (result.Status)
                return Ok(_mapper.Map<ListResultObject<StylistServiceFollowUpSettingVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("GetStylistServiceFollowUpSettingById_Base")]
        public async Task<ActionResult<RowResultObject<StylistServiceFollowUpSettingVM>>> GetStylistServiceFollowUpSettingById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _followUpSettingRep.GetStylistServiceFollowUpSettingByIdAsync(requestBody.ID);
            if (result.Status)
                return Ok(_mapper.Map<RowResultObject<StylistServiceFollowUpSettingVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("ExistStylistServiceFollowUpSetting_Base")]
        public async Task<ActionResult<BitResultObject>> ExistStylistServiceFollowUpSetting_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _followUpSettingRep.ExistStylistServiceFollowUpSettingAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("AddStylistServiceFollowUpSetting_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylistServiceFollowUpSetting_Base(AddEditStylistServiceFollowUpSettingRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var row = BuildSetting(requestBody, DateTime.Now.ToShamsi());
            var result = await _followUpSettingRep.AddStylistServiceFollowUpSettingAsync(row);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("EditStylistServiceFollowUpSetting_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylistServiceFollowUpSetting_Base(AddEditStylistServiceFollowUpSettingRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var oldRow = await _followUpSettingRep.GetStylistServiceFollowUpSettingByIdAsync(requestBody.ID);
            if (!oldRow.Status || oldRow.Result == null)
                return BadRequest(oldRow);

            var row = BuildSetting(requestBody, oldRow.Result.CreateDate);
            var result = await _followUpSettingRep.EditStylistServiceFollowUpSettingAsync(row);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteStylistServiceFollowUpSetting_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistServiceFollowUpSetting_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _followUpSettingRep.RemoveStylistServiceFollowUpSettingAsync(requestBody.ID);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        private static StylistServiceFollowUpSetting BuildSetting(AddEditStylistServiceFollowUpSettingRequestBody requestBody, DateTime? createDate)
        {
            return new StylistServiceFollowUpSetting
            {
                ID = requestBody.ID,
                CreateDate = createDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                StylistID = requestBody.StylistID,
                ServiceManagementID = requestBody.ServiceManagementID,
                StylistServicePriceVariantID = requestBody.StylistServicePriceVariantID > 0 ? requestBody.StylistServicePriceVariantID : null,
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
