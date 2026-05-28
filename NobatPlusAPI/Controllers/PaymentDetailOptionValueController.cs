using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.PaymentDetailOptionValue;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;

namespace NobatPlusAPI.Controllers
{
    [Route("PaymentDetailOptionValue")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class PaymentDetailOptionValueController : ControllerBase
    {
        private readonly IPaymentDetailOptionValueRep _rep;
        private readonly ILogRep _logRep;
        private readonly IMapper _mapper;

        public PaymentDetailOptionValueController(IPaymentDetailOptionValueRep rep, ILogRep logRep, IMapper mapper)
        {
            _rep = rep;
            _logRep = logRep;
            _mapper = mapper;
        }

        [HttpPost("GetAllPaymentDetailOptionValues_Base")]
        public async Task<ActionResult<ListResultObject<PaymentDetailOptionValueVM>>> GetAllPaymentDetailOptionValues_Base(GetPaymentDetailOptionValueListRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.GetAllPaymentDetailOptionValuesAsync(requestBody.PaymentDetailID, requestBody.ServiceOptionValueID, requestBody.PageIndex, requestBody.PageSize, requestBody.SearchText, requestBody.SortQuery);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<ListResultObject<PaymentDetailOptionValueVM>>(result));
        }

        [HttpPost("GetPaymentDetailOptionValueById_Base")]
        public async Task<ActionResult<RowResultObject<PaymentDetailOptionValueVM>>> GetPaymentDetailOptionValueById_Base(PaymentDetailOptionValueRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.GetPaymentDetailOptionValueByIdAsync(requestBody.PaymentDetailID, requestBody.ServiceOptionValueID);
            if (!result.Status) return BadRequest(result);
            return Ok(_mapper.Map<RowResultObject<PaymentDetailOptionValueVM>>(result));
        }

        [HttpPost("ExistPaymentDetailOptionValue_Base")]
        public async Task<ActionResult<BitResultObject>> ExistPaymentDetailOptionValue_Base(PaymentDetailOptionValueRowRequestBody requestBody)
        {
            if (!ModelState.IsValid) return BadRequest(requestBody);
            var result = await _rep.ExistPaymentDetailOptionValueAsync(requestBody.PaymentDetailID, requestBody.ServiceOptionValueID);
            if (string.IsNullOrEmpty(result.ErrorMessage)) return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("AddPaymentDetailOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> AddPaymentDetailOptionValues_Base(List<PaymentDetailOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new PaymentDetailOptionValue { PaymentDetailID = x.PaymentDetailID, ServiceOptionValueID = x.ServiceOptionValueID }).ToList();
            var result = await _rep.AddPaymentDetailOptionValuesAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpPut("EditPaymentDetailOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> EditPaymentDetailOptionValues_Base(List<PaymentDetailOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var rows = requestBodies.Select(x => new PaymentDetailOptionValue { PaymentDetailID = x.PaymentDetailID, ServiceOptionValueID = x.ServiceOptionValueID }).ToList();
            var result = await _rep.EditPaymentDetailOptionValuesAsync(rows);
            if (!result.Status) return BadRequest(result);
            await AddLogAsync();
            return Ok(result);
        }

        [HttpDelete("DeletePaymentDetailOptionValues_Base")]
        public async Task<ActionResult<BitResultObject>> DeletePaymentDetailOptionValues_Base(List<PaymentDetailOptionValueRowRequestBody> requestBodies)
        {
            if (!ModelState.IsValid) return BadRequest(requestBodies);
            var ids = requestBodies.Select(x => (x.PaymentDetailID, x.ServiceOptionValueID)).ToList();
            var result = await _rep.RemovePaymentDetailOptionValuesAsync(ids);
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
