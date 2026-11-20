using System.ComponentModel.DataAnnotations;
using NobatPlusDATA.DataLayer.Repositories;

namespace NobatPlusAPI.Models.Billing;

public class GetBillingListRequestBody : BillingListFilter { }

public class SaveSubscriptionPlanRequestBody
{
    public long ID { get; set; }
    [Required, StringLength(200)] public string Name { get; set; } = "";
    [Required, StringLength(10)] public string PlanType { get; set; } = "Individual";
    [Range(1, int.MaxValue)] public int MaxMemberCount { get; set; } = 1;
    [Range(1, 12)] public int DurationMonths { get; set; }
    [Range(typeof(decimal), "1", "9999999999999999")] public decimal Price { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999")] public decimal BookingFeeAmount { get; set; }
    public string CurrencyCode { get; set; } = "IRR";
    [Required, StringLength(100)] public string ContractVersion { get; set; } = "1";
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}

public class SaveBusinessBillingSettingsRequestBody
{
    [Range(1, long.MaxValue)] public long BusinessID { get; set; }
    public string DefaultFeePayer { get; set; } = "Business";
    public string CollectionMode { get; set; } = "Hybrid";
    public string InvoiceCycle { get; set; } = "Monthly";
    public bool IsBookingFeeEnabled { get; set; } = true;
    [Range(typeof(decimal), "0", "9999999999999999")] public decimal CreditLimit { get; set; }
    [Range(0, 365)] public int DueDays { get; set; } = 7;
    public bool IsActive { get; set; } = true;
}

public class SaveWorkerBillingSettingsRequestBody
{
    [Range(1, long.MaxValue)] public long BusinessID { get; set; }
    [Range(1, long.MaxValue)] public long WorkerID { get; set; }
    public string? FeePayerOverride { get; set; }
}

public class AcceptWorkerLiabilityRequestBody
{
    [Range(1, long.MaxValue)] public long BusinessID { get; set; }
    [Range(1, long.MaxValue)] public long WorkerID { get; set; }
    public bool Accept { get; set; }
    public string? ContractVersion { get; set; }
}

public class PurchaseSubscriptionRequestBody
{
    [Range(1, long.MaxValue)] public long BusinessID { get; set; }
    [Range(1, long.MaxValue)] public long SubscriptionPlanID { get; set; }
    public Guid RequestID { get; set; }
}

public class CreateBillingPaymentRequestBody
{
    [Range(1, long.MaxValue)] public long BillingAccountID { get; set; }
    public long? BillingInvoiceID { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999")] public decimal Amount { get; set; }
    public Guid RequestID { get; set; }
}

public class GetBillingAccountReportRequestBody
{
    public long WorkerID { get; set; }
}

public class IssueBillingInvoiceRequestBody
{
    [Range(1, long.MaxValue)] public long BillingAccountID { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

public class ReverseBookingPlatformFeeRequestBody
{
    [Range(1, long.MaxValue)] public long BookingPlatformFeeID { get; set; }
    [Required, StringLength(1000)] public string Reason { get; set; } = "";
}

public class RequestBillingRefundRequestBody
{
    [Range(1, long.MaxValue)] public long BillingPaymentID { get; set; }
    [Range(typeof(decimal), "1", "9999999999999999")] public decimal Amount { get; set; }
    public Guid RequestID { get; set; }
    [Required, StringLength(1000)] public string Reason { get; set; } = "";
}

public class ResolveBillingRefundRequestBody
{
    [Range(1, long.MaxValue)] public long BillingRefundID { get; set; }
    public bool Confirm { get; set; }
    [StringLength(200)] public string? BankReference { get; set; }
    [StringLength(1000)] public string Reason { get; set; } = "";
}
