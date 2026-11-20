namespace NobatPlusAPI.Models.Payment;

public class PaymentVerificationResultBody
{
    public bool Status { get; set; }
    public bool Success { get; set; }
    public bool PaymentVerified { get; set; }
    public long PaymentHistoryId { get; set; }
    public long PaymentId { get; set; }
    public string Purpose { get; set; } = "";
    public string? ProfileType { get; set; }
    public List<long> BookingIds { get; set; } = new();
    public string? TransactionCode { get; set; }
    public string? ResultPath { get; set; }
    public string? ErrorMessage { get; set; }
}
