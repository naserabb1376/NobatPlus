using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.WorkerServicePriceVariantOptionValue;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("WorkerServicePriceVariantOptionValue")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class WorkerServicePriceVariantOptionValueController : ControllerBase
    {
        private readonly IWorkerServicePriceVariantOptionValueRep _rep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public WorkerServicePriceVariantOptionValueController(IWorkerServicePriceVariantOptionValueRep rep, ILogRep logRep, IMapper mapper)
        {
            _rep = rep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllWorkerServicePriceVariantOptionValues_Base")]
        public async Task<ActionResult<ListResultObject<WorkerServicePriceVariantOptionValueVM>>> GetAllWorkerServicePriceVariantOptionValues_Base(GetWorkerServicePriceVariantOptionValueListRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.GetAllWorkerServicePriceVariantOptionValuesAsync(requestBody.WorkerServicePriceVariantID, requestBody.ServiceOptionValueID, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<ListResultObject<WorkerServicePriceVariantOptionValueVM>>(result));
        }

        [HttpPost("GetWorkerServicePriceVariantOptionValueById_Base")]
        public async Task<ActionResult<RowResultObject<WorkerServicePriceVariantOptionValueVM>>> GetWorkerServicePriceVariantOptionValueById_Base(WorkerServicePriceVariantOptionValueRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.GetWorkerServicePriceVariantOptionValueByIdAsync(requestBody.WorkerServicePriceVariantID, requestBody.ServiceOptionValueID);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<RowResultObject<WorkerServicePriceVariantOptionValueVM>>(result));
        }

        [HttpPost("ExistWorkerServicePriceVariantOptionValue_Base")]
        public async Task<ActionResult<BitResultObject>> ExistWorkerServicePriceVariantOptionValue_Base(WorkerServicePriceVariantOptionValueRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.ExistWorkerServicePriceVariantOptionValueAsync(requestBody.WorkerServicePriceVariantID, requestBody.ServiceOptionValueID);
            if (string.IsNullOrEmpty(result.ErrorMessage)) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("AddWorkerServicePriceVariantOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> AddWorkerServicePriceVariantOptionValues_Base(List<WorkerServicePriceVariantOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new WorkerServicePriceVariantOptionValue { WorkerServicePriceVariantID = x.WorkerServicePriceVariantID, ServiceOptionValueID = x.ServiceOptionValueID }).ToList();
            var result = await _rep.AddWorkerServicePriceVariantOptionValuesAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpPut("EditWorkerServicePriceVariantOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> EditWorkerServicePriceVariantOptionValues_Base(List<WorkerServicePriceVariantOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new WorkerServicePriceVariantOptionValue { WorkerServicePriceVariantID = x.WorkerServicePriceVariantID, ServiceOptionValueID = x.ServiceOptionValueID }).ToList();
            var result = await _rep.EditWorkerServicePriceVariantOptionValuesAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpDelete("DeleteWorkerServicePriceVariantOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteWorkerServicePriceVariantOptionValues_Base(List<WorkerServicePriceVariantOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var ids = requestBodies.Select(x => (x.WorkerServicePriceVariantID, x.ServiceOptionValueID)).ToList();
            var result = await _rep.RemoveWorkerServicePriceVariantOptionValuesAsync(ids);
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
