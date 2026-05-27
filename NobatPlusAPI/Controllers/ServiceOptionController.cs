using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.ServiceOption;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("ServiceOption")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ServiceOptionController : ControllerBase
    {
        private readonly IServiceOptionRep _serviceOptionRep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public ServiceOptionController(IServiceOptionRep serviceOptionRep, ILogRep logRep, IMapper mapper)
        {
            _serviceOptionRep = serviceOptionRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllServiceOptions_Base")]
        public async Task<ActionResult<ListResultObject<ServiceOptionVM>>> GetAllServiceOptions_Base(GetServiceOptionListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _serviceOptionRep.GetAllServiceOptionsAsync(requestBody.ServiceManagementID, requestBody.IsActive, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
                return Ok(_mapper.Map<ListResultObject<ServiceOptionVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("GetServiceOptionById_Base")]
        public async Task<ActionResult<RowResultObject<ServiceOptionVM>>> GetServiceOptionById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _serviceOptionRep.GetServiceOptionByIdAsync(requestBody.ID);
            if (result.Status)
                return Ok(_mapper.Map<RowResultObject<ServiceOptionVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("ExistServiceOption_Base")]
        public async Task<ActionResult<BitResultObject>> ExistServiceOption_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _serviceOptionRep.ExistServiceOptionAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("AddServiceOption_Base")]
        public async Task<ActionResult<BitResultObject>> AddServiceOption_Base(AddEditServiceOptionRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var serviceOption = new ServiceOption
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                ServiceManagementID = requestBody.ServiceManagementID,
                OptionName = requestBody.OptionName,
                OptionKey = requestBody.OptionKey,
                IsRequired = requestBody.IsRequired,
                SortOrder = requestBody.SortOrder,
                IsActive = requestBody.IsActive,
                Description = requestBody.Description
            };

            var result = await _serviceOptionRep.AddServiceOptionAsync(serviceOption);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("EditServiceOption_Base")]
        public async Task<ActionResult<BitResultObject>> EditServiceOption_Base(AddEditServiceOptionRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var oldRow = await _serviceOptionRep.GetServiceOptionByIdAsync(requestBody.ID);
            if (!oldRow.Status || oldRow.Result == null)
                return BadRequest(oldRow);

            var serviceOption = new ServiceOption
            {
                ID = requestBody.ID,
                CreateDate = oldRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ServiceManagementID = requestBody.ServiceManagementID,
                OptionName = requestBody.OptionName,
                OptionKey = requestBody.OptionKey,
                IsRequired = requestBody.IsRequired,
                SortOrder = requestBody.SortOrder,
                IsActive = requestBody.IsActive,
                Description = requestBody.Description
            };

            var result = await _serviceOptionRep.EditServiceOptionAsync(serviceOption);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteServiceOption_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteServiceOption_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _serviceOptionRep.RemoveServiceOptionAsync(requestBody.ID);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
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
