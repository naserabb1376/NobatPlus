using AutoMapper;
using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Models.PaymentHistory;
using NobatPlusAPI.Models.Public;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using NobatPlusDATA.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static NobatPlusDATA.Tools.DbTools;

namespace NobatPlusAPI.Controllers
{
    [Route("PaymentHistory")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]

    public class PaymentHistoryController : ControllerBase
    {
        IPaymentHistoryRep _PaymentHistoryRep;
        ILogRep _logRep;
        private readonly IMapper _mapper;
        private readonly IBillingRep _billingRep;


        public PaymentHistoryController(IPaymentHistoryRep PaymentHistoryRep,ILogRep logRep, IMapper mapper, IBillingRep billingRep)
        {
           _PaymentHistoryRep = PaymentHistoryRep;
           _logRep = logRep;
            _mapper = mapper;
            _billingRep = billingRep;
        }

        [HttpPost("GetAllPaymentHistories_Base")]
        [RequireRole(1, 2, 3, 4)]
        public async Task<ActionResult<ListResultObject<PaymentHistoryVM>>> GetAllPaymentHistories_Base(GetPaymentHistoryListRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentHistoryRep.GetAllPaymentHistoriesAsync(requestBody.BookingId,requestBody.PaymentId,requestBody.PageIndex,requestBody.PageSize,requestBody.SearchText,requestBody.SortQuery, User.GetCurrentRoleId() == (long) BaseRole.Admin ? 0 : User.GetCurrentUserId());
            if (result.Status)
            {
                var resultVM = _mapper.Map<ListResultObject<PaymentHistoryVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("GetPaymentHistoryById_Base")]
        [RequireRole(1, 2, 3, 4)]
        public async Task<ActionResult<RowResultObject<PaymentHistoryVM>>> GetPaymentHistoryById_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentHistoryRep.GetPaymentHistoryByIdAsync(requestBody.ID);
            if (result.Result != null && !await _billingRep.CanAccessPaymentAsync(result.Result.PaymentID, User.GetCurrentUserId(), User.GetCurrentRoleId() == (long) BaseRole.Admin)) return Forbid();
            if (result.Status)
            {
                var resultVM = _mapper.Map<RowResultObject<PaymentHistoryVM>>(result);
                return Ok(resultVM);
            }
            return BadRequest(result);
        }

        [HttpPost("ExistPaymentHistory_Base")]
        [RequireRole(4)]
        public async Task<ActionResult<BitResultObject>> ExistPaymentHistory_Base(GetRowRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            var result = await _PaymentHistoryRep.ExistPaymentHistoryAsync(requestBody.ID);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("AddPaymentHistory_Base")]
        public ActionResult<BitResultObject> AddPaymentHistory_Base(AddEditPaymentHistoryRequestBody requestBody) =>
            BadRequest(new BitResultObject { Status = false, ErrorMessage = "سوابق مالی فقط از مسیر پرداخت تاییدشده ثبت می‌شوند" });

        [HttpPut("EditPaymentHistory_Base")]
        public ActionResult<BitResultObject> EditPaymentHistory_Base(AddEditPaymentHistoryRequestBody requestBody) =>
            BadRequest(new BitResultObject { Status = false, ErrorMessage = "سوابق مالی قابل ویرایش نیستند؛ از عملیات اصلاح مالی استفاده کنید" });

        [HttpDelete("DeletePaymentHistory_Base")]
        public ActionResult<BitResultObject> DeletePaymentHistory_Base(GetRowRequestBody requestBody) =>
            BadRequest(new BitResultObject { Status = false, ErrorMessage = "حذف سابقه مالی مجاز نیست" });
    }
}
