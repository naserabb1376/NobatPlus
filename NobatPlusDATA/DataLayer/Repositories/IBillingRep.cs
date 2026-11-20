using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories;

public interface IBillingRep
{
    Task<BitResultObject> SavePlanAsync(SubscriptionPlan plan, bool isAdmin);
    Task<BitResultObject> SaveBusinessSettingsAsync(BusinessBillingSetting setting, long personId, bool isAdmin);
    Task<BitResultObject> SaveWorkerSettingsAsync(long businessId, long workerId, string? payer, decimal? revenueSharePercent, long personId, bool isAdmin);
    Task<BitResultObject> AcceptWorkerLiabilityAsync(long businessId, long workerId, bool accept, long personId, string? contractVersion = null);
    Task<RowResultObject<WorkerBillingContext>> GetWorkerBillingContextAsync(long workerId, long personId, bool isAdmin);
    Task<string?> ValidateWorkerCapacityAsync(long workerId);
    Task<BitResultObject> PurchaseSubscriptionAsync(long businessId, long planId, Guid requestId, long personId, bool isAdmin);
    Task<BitResultObject> CreateBillingPaymentAsync(long accountId, long? invoiceId, decimal amount, Guid requestId, long personId, bool isAdmin);
    Task<RowResultObject<BillingAccountReport>> GetAccountReportAsync(long workerId, long personId, bool isAdmin);
    Task<BitResultObject> IssueInvoiceAsync(long accountId, DateTime start, DateTime end, long personId, bool isAdmin);
    Task<BitResultObject> ApplyBookingStateAsync(long bookingId);
    Task<BitResultObject> ReverseFeeAsync(long feeId, string reason, long personId, bool isAdmin);
    Task<BitResultObject> ConfirmGatewayPaymentAsync(long historyId, string transactionCode, string trackingNumber, string? token, string? message);
    Task<BitResultObject> ProcessBillingAsync();
    Task AcquireLockAsync();
    Task<BitResultObject> CreateGatewayAttemptAsync(long paymentId, string gatewayName, long personId, bool isAdmin);
    Task<BitResultObject> SaveGatewayAttemptAsync(long historyId, bool success, string? url, string? message);
    Task<RowResultObject<PaymentVerificationContext>> GetPaymentVerificationContextAsync(long historyId);
    Task<bool> CanAccessPaymentAsync(long paymentId, long personId, bool isAdmin);
    Task<ListResultObject<BillingReconciliationIssue>> GetReconciliationReportAsync(bool isAdmin);
    Task<BitResultObject> RequestRefundAsync(long billingPaymentId, decimal amount, Guid requestId, string reason, long personId, bool isAdmin);
    Task<BitResultObject> ResolveRefundAsync(long refundId, bool confirm, string? reference, string reason, long personId, bool isAdmin);
}
