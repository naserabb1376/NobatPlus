namespace NobatPlusDATA.Domain;

// Private Parbad persistence; these records are not exposed as editable financial resources.
public class GatewayPaymentRecord
{
    public long ID { get; set; }
    public long TrackingNumber { get; set; }
    public decimal Amount { get; set; }
    public string Token { get; set; } = "";
    public string? TransactionCode { get; set; }
    public string GatewayName { get; set; } = "";
    public string? GatewayAccountName { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsPaid { get; set; }
}

public class GatewayTransactionRecord
{
    public long ID { get; set; }
    public long GatewayPaymentRecordID { get; set; }
    public decimal Amount { get; set; }
    public int Type { get; set; }
    public bool IsSucceed { get; set; }
    public string? Message { get; set; }
    public string? AdditionalData { get; set; }
}
