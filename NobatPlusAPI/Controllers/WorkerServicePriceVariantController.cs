using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.WorkerServicePriceVariant;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("WorkerServicePriceVariant")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class WorkerServicePriceVariantController : ControllerBase
    {
        private readonly IWorkerServicePriceVariantRep _workerServicePriceVariantRep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public WorkerServicePriceVariantController(IWorkerServicePriceVariantRep workerServicePriceVariantRep, ILogRep logRep, IMapper mapper)
        {
            _workerServicePriceVariantRep = workerServicePriceVariantRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllWorkerServicePriceVariants_Base")]
        public async Task<ActionResult<ListResultObject<WorkerServicePriceVariantVM>>> GetAllWorkerServicePriceVariants_Base(GetWorkerServicePriceVariantListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _workerServicePriceVariantRep.GetAllWorkerServicePriceVariantsAsync(requestBody.WorkerID, requestBody.ServiceManagementID, requestBody.IsActive, requestBody.OnlyLeafServices, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
                return Ok(_mapper.Map<ListResultObject<WorkerServicePriceVariantVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("GetWorkerServicePriceVariantById_Base")]
        public async Task<ActionResult<RowResultObject<WorkerServicePriceVariantVM>>> GetWorkerServicePriceVariantById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _workerServicePriceVariantRep.GetWorkerServicePriceVariantByIdAsync(requestBody.ID);
            if (result.Status)
                return Ok(_mapper.Map<RowResultObject<WorkerServicePriceVariantVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("ExistWorkerServicePriceVariant_Base")]
        public async Task<ActionResult<BitResultObject>> ExistWorkerServicePriceVariant_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _workerServicePriceVariantRep.ExistWorkerServicePriceVariantAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("AddWorkerServicePriceVariant_Base")]
        public async Task<ActionResult<BitResultObject>> AddWorkerServicePriceVariant_Base(AddEditWorkerServicePriceVariantRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var variant = BuildVariant(requestBody, DateTime.Now.ToShamsi());
            var result = await _workerServicePriceVariantRep.AddWorkerServicePriceVariantAsync(variant);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("EditWorkerServicePriceVariant_Base")]
        public async Task<ActionResult<BitResultObject>> EditWorkerServicePriceVariant_Base(AddEditWorkerServicePriceVariantRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var oldRow = await _workerServicePriceVariantRep.GetWorkerServicePriceVariantByIdAsync(requestBody.ID);
            if (!oldRow.Status || oldRow.Result == null)
                return BadRequest(oldRow);

            var variant = BuildVariant(requestBody, oldRow.Result.CreateDate);
            var result = await _workerServicePriceVariantRep.EditWorkerServicePriceVariantAsync(variant);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteWorkerServicePriceVariant_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteWorkerServicePriceVariant_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _workerServicePriceVariantRep.RemoveWorkerServicePriceVariantAsync(requestBody.ID);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        private static WorkerServicePriceVariant BuildVariant(AddEditWorkerServicePriceVariantRequestBody requestBody, DateTime? createDate)
        {
            return new WorkerServicePriceVariant
            {
                ID = requestBody.ID,
                CreateDate = createDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                WorkerID = requestBody.WorkerID,
                ServiceManagementID = requestBody.ServiceManagementID,
                Price = requestBody.Price,
                Duration = requestBody.Duration,
                DepositPercent = requestBody.DepositPercent,
                IsActive = requestBody.IsActive,
                BookingTagID = requestBody.BookingTagID,
                Description = requestBody.Description,
                OptionValueCombinationKey = WorkerServicePriceVariant.BuildOptionValueCombinationKey(requestBody.OptionValueIDs),
                OptionValues = requestBody.OptionValueIDs
                    .Where(x => x > 0)
                    .Distinct()
                    .Select(x => new WorkerServicePriceVariantOptionValue
                    {
                        WorkerServicePriceVariantID = requestBody.ID,
                        ServiceOptionValueID = x
                    })
                    .ToList()
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
