using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NobatPlusAPI.Models.Billing;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusAPI.Controllers;

[ApiController]
[Route("Billing")]
[Authorize]
[RequireRole(2, 3, 4)]
[Produces("application/json")]
public class BillingController : ControllerBase
{
    private readonly IBillingRep _billing;
    public BillingController(IBillingRep billing) => _billing = billing;
    private long PersonID => User.GetCurrentUserId();
    private bool IsAdmin => User.GetCurrentRoleId() == 4;

    private async Task<ActionResult<BitResultObject>> ExecuteAsync(Func<Task<BitResultObject>> operation)
    {
        try
        {
            var result = await operation();
            return result.Status ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new BitResultObject { Status = false, ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}" });
        }
    }

    [HttpPost("SaveSubscriptionPlan_Base")]
    [RequireRole(4)]
    public Task<ActionResult<BitResultObject>> SaveSubscriptionPlan_Base(SaveSubscriptionPlanRequestBody body) => ExecuteAsync(() =>
        _billing.SavePlanAsync(new SubscriptionPlan
        {
            ID = body.ID, Name = body.Name, DurationMonths = body.DurationMonths, Price = body.Price,
            BookingFeeAmount = body.BookingFeeAmount, CurrencyCode = body.CurrencyCode,
            ContractVersion = body.ContractVersion, IsActive = body.IsActive, Description = body.Description
        }, IsAdmin));

    [HttpPost("SaveBusinessBillingSettings_Base")]
    public Task<ActionResult<BitResultObject>> SaveBusinessBillingSettings_Base(SaveBusinessBillingSettingsRequestBody body) => ExecuteAsync(() =>
        _billing.SaveBusinessSettingsAsync(new BusinessBillingSetting
        {
            BusinessID = body.BusinessID, DefaultFeePayer = body.DefaultFeePayer, CollectionMode = body.CollectionMode,
            InvoiceCycle = body.InvoiceCycle, IsBookingFeeEnabled = body.IsBookingFeeEnabled,
            CreditLimit = body.CreditLimit, DueDays = body.DueDays, IsActive = body.IsActive
        }, PersonID, IsAdmin));

    [HttpPost("SaveWorkerBillingSettings_Base")]
    public Task<ActionResult<BitResultObject>> SaveWorkerBillingSettings_Base(SaveWorkerBillingSettingsRequestBody body) => ExecuteAsync(() =>
        _billing.SaveWorkerSettingsAsync(body.BusinessID, body.WorkerID, body.FeePayerOverride, PersonID, IsAdmin));

    [HttpPost("AcceptWorkerBillingLiability_Base")]
    public Task<ActionResult<BitResultObject>> AcceptWorkerBillingLiability_Base(AcceptWorkerLiabilityRequestBody body) => ExecuteAsync(() =>
        _billing.AcceptWorkerLiabilityAsync(body.BusinessID, body.WorkerID, body.Accept, PersonID, body.ContractVersion));

    [HttpPost("GetWorkerBillingContext_Base")]
    public async Task<ActionResult<RowResultObject<WorkerBillingContext>>> GetWorkerBillingContext_Base(GetBillingAccountReportRequestBody body)
    {
        var result = await _billing.GetWorkerBillingContextAsync(body.WorkerID > 0 ? body.WorkerID : User.GetCurrentProfileId(), PersonID, IsAdmin);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("PurchaseSubscription_Base")]
    public Task<ActionResult<BitResultObject>> PurchaseSubscription_Base(PurchaseSubscriptionRequestBody body) => ExecuteAsync(() =>
        _billing.PurchaseSubscriptionAsync(body.BusinessID, body.SubscriptionPlanID, body.RequestID, PersonID, IsAdmin));

    [HttpPost("CreateBillingPayment_Base")]
    public Task<ActionResult<BitResultObject>> CreateBillingPayment_Base(CreateBillingPaymentRequestBody body) => ExecuteAsync(() =>
        _billing.CreateBillingPaymentAsync(body.BillingAccountID, body.BillingInvoiceID, body.Amount, body.RequestID, PersonID, IsAdmin));

    [HttpPost("GetBillingAccountReport_Base")]
    public async Task<ActionResult<RowResultObject<BillingAccountReport>>> GetBillingAccountReport_Base(GetBillingAccountReportRequestBody body)
    {
        try
        {
            var workerId = body.WorkerID > 0 ? body.WorkerID : User.GetCurrentProfileId();
            var result = await _billing.GetAccountReportAsync(workerId, PersonID, IsAdmin);
            return result.Status ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new RowResultObject<BillingAccountReport> { Status = false, ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}" });
        }
    }

    [HttpPost("IssueBillingInvoice_Base")]
    public Task<ActionResult<BitResultObject>> IssueBillingInvoice_Base(IssueBillingInvoiceRequestBody body) => ExecuteAsync(() =>
        _billing.IssueInvoiceAsync(body.BillingAccountID, body.PeriodStart, body.PeriodEnd, PersonID, IsAdmin));

    [HttpPost("ReverseBookingPlatformFee_Base")]
    [RequireRole(4)]
    public Task<ActionResult<BitResultObject>> ReverseBookingPlatformFee_Base(ReverseBookingPlatformFeeRequestBody body) => ExecuteAsync(() =>
        _billing.ReverseFeeAsync(body.BookingPlatformFeeID, body.Reason, PersonID, IsAdmin));

    [HttpPost("RunBillingNow_Base")]
    [RequireRole(4)]
    public async Task<ActionResult<BitResultObject>> RunBillingNow_Base()
    {
        if (!IsAdmin) return Forbid();
        return await ExecuteAsync(_billing.ProcessBillingAsync);
    }

    [HttpPost("GetBillingReconciliationReport_Base")]
    [RequireRole(4)]
    public async Task<ActionResult<ListResultObject<BillingReconciliationIssue>>> GetBillingReconciliationReport_Base()
    {
        if (!IsAdmin) return Forbid();
        var result = await _billing.GetReconciliationReportAsync(true);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    [HttpPost("RequestBillingRefund_Base")]
    public Task<ActionResult<BitResultObject>> RequestBillingRefund_Base(RequestBillingRefundRequestBody body) => ExecuteAsync(() =>
        _billing.RequestRefundAsync(body.BillingPaymentID, body.Amount, body.RequestID, body.Reason, PersonID, IsAdmin));

    [HttpPost("ResolveBillingRefund_Base")]
    [RequireRole(4)]
    public Task<ActionResult<BitResultObject>> ResolveBillingRefund_Base(ResolveBillingRefundRequestBody body) => ExecuteAsync(() =>
        _billing.ResolveRefundAsync(body.BillingRefundID, body.Confirm, body.BankReference, body.Reason, PersonID, IsAdmin));
}
