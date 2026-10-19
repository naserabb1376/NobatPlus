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
        private readonly IWorkerRep _workerRep;
        private readonly ILogRep _logRep;

        public FinancialAccountController(IFinancialAccountRep financialAccountRep, IWorkerRep workerRep, ILogRep logRep)
        {
            _financialAccountRep = financialAccountRep;
            _workerRep = workerRep;
            _logRep = logRep;
        }

        [HttpPost("GetMyFinancialAccount_Base")]
        public async Task<ActionResult<RowResultObject<FinancialAccountReport>>> GetMyFinancialAccount_Base(GetFinancialAccountRequestBody requestBody)
        {
            var worker = await GetCurrentWorkerAsync();
            if (!worker.Status)
            {
                return BadRequest(new RowResultObject<FinancialAccountReport> { Status = false, ErrorMessage = "خدمات دهنده/محل خدمت یافت نشد" });
            }

            var result = await _financialAccountRep.GetFinancialAccountAsync(
                worker.ID,
                requestBody.PageIndex,
                requestBody.PageSize,
                requestBody.FromDate?.ToShamsi(),
                requestBody.ToDate?.ToShamsi(),
                requestBody.TransactionType ?? "");
            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("GetWorkerFinancialAccount_Base")]
        public async Task<ActionResult<RowResultObject<FinancialAccountReport>>> GetWorkerFinancialAccount_Base(GetWorkerFinancialAccountRequestBody requestBody)
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
                requestBody.WorkerId,
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
            var worker = await GetCurrentWorkerAsync();
            if (!worker.Status)
            {
                return BadRequest(new BitResultObject { Status = false, ErrorMessage = "خدمات دهنده/محل خدمت یافت نشد" });
            }

            var result = await _financialAccountRep.UpdateBankInfoAsync(
                worker.ID,
                requestBody.Iban ?? "",
                requestBody.BankAccountOwnerName ?? "");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("AdminUpdateBankInfo_Base")]
        public async Task<ActionResult<BitResultObject>> AdminUpdateBankInfo_Base(AdminUpdateFinancialBankInfoRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = await _financialAccountRep.UpdateBankInfoAsync(
                requestBody.WorkerId,
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

            var worker = await GetCurrentWorkerAsync();
            if (!worker.Status)
            {
                return BadRequest(new BitResultObject { Status = false, ErrorMessage = "خدمات دهنده/محل خدمت یافت نشد" });
            }

            var result = await _financialAccountRep.RequestSettlementAsync(
                worker.ID,
                requestBody.Amount,
                requestBody.Description ?? "درخواست تسویه");

            return result.Status ? Ok(result) : BadRequest(result);
        }

        [HttpPost("AdminRequestSettlement_Base")]
        public async Task<ActionResult<BitResultObject>> AdminRequestSettlement_Base(AdminRequestSettlementRequestBody requestBody)
        {
            if (User.GetCurrentRoleId() != 4)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var result = await _financialAccountRep.RequestSettlementAsync(
                requestBody.WorkerId,
                requestBody.Amount,
                requestBody.Description ?? "درخواست تسویه ادمین");

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

        private async Task<BitResultObject> GetCurrentWorkerAsync()
        {
            var profileId = User.GetCurrentProfileId();
            if (profileId <= 0)
            {
                var profiles = await _workerRep.GetWorkerProfilesByPersonIdAsync(User.GetCurrentUserId());
                var profileType = User.GetCurrentRoleId() == (long)DbTools.BaseRole.WorkShop ? nameof(DbTools.BaseRole.WorkShop).ToLower() : nameof(DbTools.BaseRole.Worker).ToLower();
                profileId = profiles.Results?.FirstOrDefault(x => x.ProfileType == profileType)?.ID ?? 0;
            }

            return new BitResultObject
            {
                ID = profileId,
                Status = profileId > 0,
                ErrorMessage = profileId > 0 ? "" : "پروفایل فعال یافت نشد"
            };
        }
    }
}
