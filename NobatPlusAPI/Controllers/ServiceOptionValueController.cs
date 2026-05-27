using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.ServiceOptionValue;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("ServiceOptionValue")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class ServiceOptionValueController : ControllerBase
    {
        private readonly IServiceOptionValueRep _serviceOptionValueRep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public ServiceOptionValueController(IServiceOptionValueRep serviceOptionValueRep, ILogRep logRep, IMapper mapper)
        {
            _serviceOptionValueRep = serviceOptionValueRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllServiceOptionValues_Base")]
        public async Task<ActionResult<ListResultObject<ServiceOptionValueVM>>> GetAllServiceOptionValues_Base(GetServiceOptionValueListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _serviceOptionValueRep.GetAllServiceOptionValuesAsync(requestBody.ServiceOptionID, requestBody.IsActive, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
                return Ok(_mapper.Map<ListResultObject<ServiceOptionValueVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("GetServiceOptionValueById_Base")]
        public async Task<ActionResult<RowResultObject<ServiceOptionValueVM>>> GetServiceOptionValueById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _serviceOptionValueRep.GetServiceOptionValueByIdAsync(requestBody.ID);
            if (result.Status)
                return Ok(_mapper.Map<RowResultObject<ServiceOptionValueVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("ExistServiceOptionValue_Base")]
        public async Task<ActionResult<BitResultObject>> ExistServiceOptionValue_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _serviceOptionValueRep.ExistServiceOptionValueAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("AddServiceOptionValue_Base")]
        public async Task<ActionResult<BitResultObject>> AddServiceOptionValue_Base(AddEditServiceOptionValueRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var serviceOptionValue = new ServiceOptionValue
            {
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                ServiceOptionID = requestBody.ServiceOptionID,
                ValueName = requestBody.ValueName,
                SortOrder = requestBody.SortOrder,
                IsActive = requestBody.IsActive,
                Description = requestBody.Description
            };

            var result = await _serviceOptionValueRep.AddServiceOptionValueAsync(serviceOptionValue);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("EditServiceOptionValue_Base")]
        public async Task<ActionResult<BitResultObject>> EditServiceOptionValue_Base(AddEditServiceOptionValueRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var oldRow = await _serviceOptionValueRep.GetServiceOptionValueByIdAsync(requestBody.ID);
            if (!oldRow.Status || oldRow.Result == null)
                return BadRequest(oldRow);

            var serviceOptionValue = new ServiceOptionValue
            {
                ID = requestBody.ID,
                CreateDate = oldRow.Result.CreateDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                ServiceOptionID = requestBody.ServiceOptionID,
                ValueName = requestBody.ValueName,
                SortOrder = requestBody.SortOrder,
                IsActive = requestBody.IsActive,
                Description = requestBody.Description
            };

            var result = await _serviceOptionValueRep.EditServiceOptionValueAsync(serviceOptionValue);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteServiceOptionValue_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteServiceOptionValue_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _serviceOptionValueRep.RemoveServiceOptionValueAsync(requestBody.ID);
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
