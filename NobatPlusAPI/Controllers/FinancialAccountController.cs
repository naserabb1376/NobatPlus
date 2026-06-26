using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.FinancialAccount;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using Domains;

namespace NobatPlusAPI.Controllers
{
    [Route("FinancialAccount")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class FinancialAccountController : ControllerBase
    {
        private readonly IFinancialAccountRep _financialAccountRep;
        private readonly IStylistRep _stylistRep;
        private readonly ILogRep _logRep;

        public FinancialAccountController(IFinancialAccountRep financialAccountRep, IStylistRep stylistRep, ILogRep logRep)
        {
            _financialAccountRep = financialAccountRep;
            _stylistRep = stylistRep;
            _logRep = logRep;
        }

        [HttpPost("GetMyFinancialAccount_Base")]
        public async Task<ActionResult<RowResultObject<FinancialAccountReport>>> GetMyFinancialAccount_Base(GetFinancialAccountRequestBody requestBody)
        {
            var stylist = await GetCurrentStylistAsync();
            if (!stylist.Status)
            {
                return BadRequest(new RowResultObject<FinancialAccountReport> { Status = false, ErrorMessage = "آرایشگر/سالن یافت نشد" });
            }

            var result = await _financialAccountRep.GetFinancialAccountAsync(
                stylist.ID,
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.FromDate?.ToShamsi(),
                requestBody.ToDate?.ToShamsi(),
                requestBody.TransactionType ?? "");
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("GetStylistFinancialAccount_Base")]
        public async Task<ActionResult<RowResultObject<FinancialAccountReport>>> GetStylistFinancialAccount_Base(GetStylistFinancialAccountRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = await _financialAccountRep.GetFinancialAccountAsync(
                requestBody.StylistId,
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.FromDate?.ToShamsi(),
                requestBody.ToDate?.ToShamsi(),
                requestBody.TransactionType ?? "");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("UpdateBankInfo_Base")]
        public async Task<ActionResult<BitResultObject>> UpdateBankInfo_Base(UpdateFinancialBankInfoRequestBody requestBody)
        {
            var stylist = await GetCurrentStylistAsync();
            if (!stylist.Status)
            {
                return BadRequest(new BitResultObject { Status = false, ErrorMessage = "آرایشگر/سالن یافت نشد" });
            }

            var result = await _financialAccountRep.UpdateBankInfoAsync(
                stylist.ID,
                requestBody.Iban ?? "",
                requestBody.BankAccountOwnerName ?? "");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("RequestSettlement_Base")]
        public async Task<ActionResult<BitResultObject>> RequestSettlement_Base(RequestSettlementRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var stylist = await GetCurrentStylistAsync();
            if (!stylist.Status)
            {
                return BadRequest(new BitResultObject { Status = false, ErrorMessage = "آرایشگر/سالن یافت نشد" });
            }

            var result = await _financialAccountRep.RequestSettlementAsync(
                stylist.ID,
                requestBody.Amount,
                requestBody.Description ?? "درخواست تسویه");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("GetSettlementRequests_Base")]
        public async Task<ActionResult<ListResultObject<AdminSettlementRequestReport>>> GetSettlementRequests_Base(GetSettlementRequestsRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4)
            {
                return Forbid();
            }

            var result = await _financialAccountRep.GetSettlementRequestsAsync(
                requestBody.Status ?? "",
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.SearchText ?? "",
                requestBody.AccountType ?? "",
                requestBody.FromDate?.ToShamsi(),
                requestBody.ToDate?.ToShamsi(),
                requestBody.SortQuery ?? "");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("UpdateSettlementStatus_Base")]
        public async Task<ActionResult<BitResultObject>> UpdateSettlementStatus_Base(UpdateSettlementStatusRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = await _financialAccountRep.UpdateSettlementStatusAsync(
                requestBody.SettlementRequestID,
                requestBody.Status,
                requestBody.TrackingCode ?? "",
                requestBody.RejectReason ?? "");

            if (result.Status)
            {
                await _logRep.AddLogAsync(new Log
                {
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    LogTime = DateTime.Now.ToShamsi(),
                    ActionName = "FinancialAccount/UpdateSettlementStatus_Base",
                    Description = $"adminPersonId={User.GetCurrentUserId()}; settlementRequestId={requestBody.SettlementRequestID}; status={requestBody.Status}; trackingCode={requestBody.TrackingCode}; rejectReason={requestBody.RejectReason}"
                });
            }

            return result.Status ? Ok(result) : BadRequest(result);
        }

        private async Task<BitResultObject> GetCurrentStylistAsync()
        {
            var userId = User.GetCurrentUserId();
            return await _stylistRep.ExistStylistAsync(userId.ToString(), "personid");
        }
    }
}
