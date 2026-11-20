using Domains;

namespace NobatPlusDATA.Domain;

public static class PaymentPurposes
{
    public const string Service = "service";
    public const string Subscription = "subscription";
    public const string BillingCredit = "billing_credit";
    public const string BillingInvoice = "billing_invoice";
}

public class PaymentVerificationContext
{
    public long PaymentHistoryID { get; set; }
    public long PaymentID { get; set; }
    public string Purpose { get; set; } = "";
    public string? ProfileType { get; set; }
    public List<long> BookingIDs { get; set; } = new();
}

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = "";
    public string PlanType { get; set; } = "Individual";
    public int MaxMemberCount { get; set; } = 1;
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public decimal BookingFeeAmount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string ContractVersion { get; set; } = "1";
}

public class BusinessSubscription : BaseEntity
{
    public long BusinessID { get; set; }
    public long SubscriptionPlanID { get; set; }
    public long PaymentID { get; set; }
    public Guid RequestID { get; set; }
    public string PlanNameSnapshot { get; set; } = "";
    public string PlanTypeSnapshot { get; set; } = "Individual";
    public int MaxMemberCountSnapshot { get; set; } = 1;
    public int DurationMonthsSnapshot { get; set; }
    public decimal PriceSnapshot { get; set; }
    public decimal BookingFeeAmountSnapshot { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string ContractVersionSnapshot { get; set; } = "";
    public string Status { get; set; } = "pending";
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
}

public class BillingAccount : BaseEntity
{
    public long WorkerID { get; set; }
    public string OwnerType { get; set; } = "Worker";
    public string CurrencyCode { get; set; } = "IRR";
}

public class BusinessBillingSetting : BaseEntity
{
    public long BusinessID { get; set; }
    public string DefaultFeePayer { get; set; } = "Business";
    public string CollectionMode { get; set; } = "Hybrid";
    public string InvoiceCycle { get; set; } = "Monthly";
    public string SettlementDestination { get; set; } = "Business";
    public bool IsBookingFeeEnabled { get; set; } = true;
    public decimal CreditLimit { get; set; }
    public int DueDays { get; set; } = 7;
    public DateTime EffectiveFrom { get; set; }
}

public class WorkerBillingSetting : BaseEntity
{
    public long BusinessID { get; set; }
    public long WorkerID { get; set; }
    public string? FeePayerOverride { get; set; }
    public decimal RevenueSharePercent { get; set; }
    public long? AcceptedByPersonID { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? AcceptedContractVersion { get; set; }
}

public class BookingPlatformFee : BaseEntity
{
    public long BookingID { get; set; }
    public long BusinessID { get; set; }
    public long WorkerID { get; set; }
    public long BillingAccountID { get; set; }
    public long BusinessSubscriptionID { get; set; }
    public decimal FeeAmount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string FeePayerSnapshot { get; set; } = "";
    public string CollectionModeSnapshot { get; set; } = "";
    public string ContractVersionSnapshot { get; set; } = "";
    public string Status { get; set; } = "accrued";
    public DateTime AccruedAt { get; set; }
    public DateTime? ReversedAt { get; set; }
}

public class BillingTransaction : BaseEntity
{
    public long BillingAccountID { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = "";
    public string SourceType { get; set; } = "";
    public long SourceID { get; set; }
    public DateTime OccurredAt { get; set; }
}

public class BillingInvoice : BaseEntity
{
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal RemainingAmount { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal AllocatedAmount { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal ReversedAmount { get; set; }
    public long BillingAccountID { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "open";
    public string CurrencyCode { get; set; } = "IRR";
    public decimal TotalAmount { get; set; }
}

public class BillingInvoiceItem : BaseEntity
{
    public long BillingInvoiceID { get; set; }
    public long BookingPlatformFeeID { get; set; }
    public decimal Amount { get; set; }
}

public class BillingPayment : BaseEntity
{
    public long BillingAccountID { get; set; }
    public long PaymentID { get; set; }
    public long? BillingInvoiceID { get; set; }
    public Guid RequestID { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string Status { get; set; } = "pending";
    public DateTime? ConfirmedAt { get; set; }
}

public class BillingAllocation : BaseEntity
{
    public long BillingPaymentID { get; set; }
    public long BookingPlatformFeeID { get; set; }
    public decimal Amount { get; set; }
    public DateTime AllocatedAt { get; set; }
    public DateTime? ReversedAt { get; set; }
}

public class BillingRefund : BaseEntity
{
    public long BillingPaymentID { get; set; }
    public Guid RequestID { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public long RequestedByPersonID { get; set; }
    public long? ResolvedByPersonID { get; set; }
    public string? BankReference { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class BillingSettingHistory : BaseEntity
{
    public long BusinessID { get; set; }
    public long? WorkerID { get; set; }
    public long ActorPersonID { get; set; }
    public string BeforeJson { get; set; } = "";
    public string AfterJson { get; set; } = "";
    public DateTime OccurredAt { get; set; }
}

public class BillingAccountReport
{
    public long BillingAccountID { get; set; }
    public long WorkerID { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public decimal NetBalance { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AvailableCredit { get; set; }
    public decimal OpenFees { get; set; }
    public decimal TotalFees { get; set; }
    public decimal ReversedFees { get; set; }
    public decimal ConfirmedPayments { get; set; }
    public decimal UnallocatedCredit { get; set; }
    public decimal ReservedCredit { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal ReconciliationDifference { get; set; }
    public bool IsReconciled { get; set; }
}

public class BillingReconciliationIssue
{
    public string IssueType { get; set; } = "";
    public long? PaymentID { get; set; }
    public long? PaymentHistoryID { get; set; }
    public long? BillingAccountID { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal ActualAmount { get; set; }
}

public class WorkerBillingContext
{
    public long WorkerID { get; set; }
    public long BusinessID { get; set; }
    public bool IsBookingFeeEnabled { get; set; }
    public bool HasActiveSubscription { get; set; }
    public decimal BookingFeeAmount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    public string? ContractVersion { get; set; }
    public string FeePayer { get; set; } = "Business";
    public string CollectionMode { get; set; } = "Hybrid";
    public bool RequiresWorkerConsent { get; set; }
    public bool HasWorkerConsent { get; set; }
    public long? SubscriptionID { get; set; }
    public long? SubscriptionPlanID { get; set; }
    public string? PlanType { get; set; }
    public int TotalCapacity { get; set; }
    public int RegisteredMemberCount { get; set; }
    public int ConsumedMemberCount { get; set; }
    public int RemainingCapacity { get; set; }
    public bool IsSubscriptionValid { get; set; }
    public bool IsProfileCovered { get; set; }
    public string? CoverageReason { get; set; }
}
