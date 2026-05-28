using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.StylistServicePriceVariantOptionValue;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("StylistServicePriceVariantOptionValue")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class StylistServicePriceVariantOptionValueController : ControllerBase
    {
        private readonly IStylistServicePriceVariantOptionValueRep _rep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public StylistServicePriceVariantOptionValueController(IStylistServicePriceVariantOptionValueRep rep, ILogRep logRep, IMapper mapper)
        {
            _rep = rep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllStylistServicePriceVariantOptionValues_Base")]
        public async Task<ActionResult<ListResultObject<StylistServicePriceVariantOptionValueVM>>> GetAllStylistServicePriceVariantOptionValues_Base(GetStylistServicePriceVariantOptionValueListRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.GetAllStylistServicePriceVariantOptionValuesAsync(requestBody.StylistServicePriceVariantID, requestBody.ServiceOptionValueID, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<ListResultObject<StylistServicePriceVariantOptionValueVM>>(result));
        }

        [HttpPost("GetStylistServicePriceVariantOptionValueById_Base")]
        public async Task<ActionResult<RowResultObject<StylistServicePriceVariantOptionValueVM>>> GetStylistServicePriceVariantOptionValueById_Base(StylistServicePriceVariantOptionValueRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.GetStylistServicePriceVariantOptionValueByIdAsync(requestBody.StylistServicePriceVariantID, requestBody.ServiceOptionValueID);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<RowResultObject<StylistServicePriceVariantOptionValueVM>>(result));
        }

        [HttpPost("ExistStylistServicePriceVariantOptionValue_Base")]
        public async Task<ActionResult<BitResultObject>> ExistStylistServicePriceVariantOptionValue_Base(StylistServicePriceVariantOptionValueRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.ExistStylistServicePriceVariantOptionValueAsync(requestBody.StylistServicePriceVariantID, requestBody.ServiceOptionValueID);
            if (string.IsNullOrEmpty(result.ErrorMessage)) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("AddStylistServicePriceVariantOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> AddStylistServicePriceVariantOptionValues_Base(List<StylistServicePriceVariantOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new StylistServicePriceVariantOptionValue { StylistServicePriceVariantID = x.StylistServicePriceVariantID, ServiceOptionValueID = x.ServiceOptionValueID }).ToList();
            var result = await _rep.AddStylistServicePriceVariantOptionValuesAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpPut("EditStylistServicePriceVariantOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> EditStylistServicePriceVariantOptionValues_Base(List<StylistServicePriceVariantOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new StylistServicePriceVariantOptionValue { StylistServicePriceVariantID = x.StylistServicePriceVariantID, ServiceOptionValueID = x.ServiceOptionValueID }).ToList();
            var result = await _rep.EditStylistServicePriceVariantOptionValuesAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpDelete("DeleteStylistServicePriceVariantOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> DeleteStylistServicePriceVariantOptionValues_Base(List<StylistServicePriceVariantOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var ids = requestBodies.Select(x => (x.StylistServicePriceVariantID, x.ServiceOptionValueID)).ToList();
            var result = await _rep.RemoveStylistServicePriceVariantOptionValuesAsync(ids);
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
