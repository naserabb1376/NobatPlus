using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Models.StylistServicePriceVariant;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("StylistServicePriceVariant")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class StylistServicePriceVariantController : ControllerBase
    {
        private readonly IStylistServicePriceVariantRep _stylistServicePriceVariantRep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public StylistServicePriceVariantController(IStylistServicePriceVariantRep stylistServicePriceVariantRep, ILogRep logRep, IMapper mapper)
        {
            _stylistServicePriceVariantRep = stylistServicePriceVariantRep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllStylistServicePriceVariants_Base")]
        public async Task<ActionResult<ListResultObject<StylistServicePriceVariantVM>>> GetAllStylistServicePriceVariants_Base(GetStylistServicePriceVariantListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _stylistServicePriceVariantRep.GetAllStylistServicePriceVariantsAsync(requestBody.StylistID, requestBody.ServiceManagementID, requestBody.IsActive, requestBody.OnlyLeafServices, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (result.Status)
                return Ok(_mapper.Map<ListResultObject<StylistServicePriceVariantVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("GetStylistServicePriceVariantById_Base")]
        public async Task<ActionResult<RowResultObject<StylistServicePriceVariantVM>>> GetStylistServicePriceVariantById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _stylistServicePriceVariantRep.GetStylistServicePriceVariantByIdAsync(requestBody.ID);
            if (result.Status)
                return Ok(_mapper.Map<RowResultObject<StylistServicePriceVariantVM>>(result));

            return BadRequest(result);
        }

        [HttpPost("ExistStylistServicePriceVariant_Base")]
        public async Task<ActionResult<BitResultObject>> ExistStylistServicePriceVariant_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _stylistServicePriceVariantRep.ExistStylistServicePriceVariantAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("AddStylistServicePriceVariant_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylistServicePriceVariant_Base(AddEditStylistServicePriceVariantRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var variant = BuildVariant(requestBody, DateTime.Now.ToShamsi());
            var result = await _stylistServicePriceVariantRep.AddStylistServicePriceVariantAsync(variant);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut("EditStylistServicePriceVariant_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylistServicePriceVariant_Base(AddEditStylistServicePriceVariantRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var oldRow = await _stylistServicePriceVariantRep.GetStylistServicePriceVariantByIdAsync(requestBody.ID);
            if (!oldRow.Status || oldRow.Result == null)
                return BadRequest(oldRow);

            var variant = BuildVariant(requestBody, oldRow.Result.CreateDate);
            var result = await _stylistServicePriceVariantRep.EditStylistServicePriceVariantAsync(variant);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("DeleteStylistServicePriceVariant_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistServicePriceVariant_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var result = await _stylistServicePriceVariantRep.RemoveStylistServicePriceVariantAsync(requestBody.ID);
            if (result.Status)
            {
                await AddLogAsync();
                return Ok(result);
            }

            return BadRequest(result);
        }

        private static StylistServicePriceVariant BuildVariant(AddEditStylistServicePriceVariantRequestBody requestBody, DateTime? createDate)
        {
            return new StylistServicePriceVariant
            {
                ID = requestBody.ID,
                CreateDate = createDate,
                UpdateDate = DateTime.Now.ToShamsi(),
                StylistID = requestBody.StylistID,
                ServiceManagementID = requestBody.ServiceManagementID,
                Price = requestBody.Price,
                Duration = requestBody.Duration,
                DepositPercent = requestBody.DepositPercent,
                IsActive = requestBody.IsActive,
                Description = requestBody.Description,
                OptionValueCombinationKey = StylistServicePriceVariant.BuildOptionValueCombinationKey(requestBody.OptionValueIDs),
                OptionValues = requestBody.OptionValueIDs
                    .Where(x => x > 0)
                    .Distinct()
                    .Select(x => new StylistServicePriceVariantOptionValue
                    {
                        StylistServicePriceVariantID = requestBody.ID,
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
