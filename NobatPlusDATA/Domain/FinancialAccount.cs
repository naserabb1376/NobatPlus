using Domains;

namespace NobatPlusDATA.Domain
{
    public class FinancialAccount : BaseEntity
    {
        public long StylistID { get; set; }
        public string AccountType { get; set; } = "";
        public decimal Balance { get; set; }
        public string Iban { get; set; } = "";
        public string BankAccountOwnerName { get; set; } = "";
        public bool IsActive { get; set; } = true;

        public Stylist Stylist { get; set; }
        public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
        public ICollection<SettlementRequest> SettlementRequests { get; set; } = new List<SettlementRequest>();
    }

    public class FinancialTransaction : BaseEntity
    {
        public long FinancialAccountID { get; set; }
        public long? BookingID { get; set; }
        public long? PaymentID { get; set; }
        public long? SettlementRequestID { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public string ReferenceNumber { get; set; } = "";

        public FinancialAccount FinancialAccount { get; set; }
        public Booking? Booking { get; set; }
        public Payment? Payment { get; set; }
        public SettlementRequest? SettlementRequest { get; set; }
    }

    public class SettlementRequest : BaseEntity
    {
        public long FinancialAccountID { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public DateTime RequestDate { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string Iban { get; set; } = "";
        public string BankAccountOwnerName { get; set; } = "";
        public string TrackingCode { get; set; } = "";
        public string RejectReason { get; set; } = "";

        public FinancialAccount FinancialAccount { get; set; }
        public ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
    }

    public class FinancialAccountReport
    {
        public long AccountId { get; set; }
        public long StylistId { get; set; }
        public string AccountType { get; set; } = "";
        public decimal Balance { get; set; }
        public decimal PendingSettlementAmount { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal TotalEarned { get; set; }
        public decimal TotalSettled { get; set; }
        public string Iban { get; set; } = "";
        public string BankAccountOwnerName { get; set; } = "";
        public List<FinancialTransactionReport> Transactions { get; set; } = new();
        public List<SettlementRequestReport> SettlementRequests { get; set; } = new();
    }

    public class FinancialTransactionReport
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public long? BookingId { get; set; }
        public long? PaymentId { get; set; }
        public string Description { get; set; } = "";
        public string ReferenceNumber { get; set; } = "";
    }

    public class SettlementRequestReport
    {
        public long Id { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = "";
        public DateTime RequestDate { get; set; }
        public DateTime? SettlementDate { get; set; }
        public string TrackingCode { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class AdminSettlementRequestReport : SettlementRequestReport
    {
        public long FinancialAccountId { get; set; }
        public long StylistId { get; set; }
        public string AccountType { get; set; } = "";
        public string StylistName { get; set; } = "";
        public string PersonFullName { get; set; } = "";
        public string Iban { get; set; } = "";
        public string BankAccountOwnerName { get; set; } = "";
        public string RejectReason { get; set; } = "";
    }
}
