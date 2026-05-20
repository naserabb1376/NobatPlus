using Domains;

namespace NobatPlusDATA.Domain
{
    public class Wallet : BaseEntity
    {
        public long CustomerID { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; } = true;

        public Customer Customer { get; set; }
        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }

    public class WalletTransaction : BaseEntity
    {
        public long WalletID { get; set; }
        public long? BookingID { get; set; }
        public long? PaymentID { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public string ReferenceNumber { get; set; } = "";

        public Wallet Wallet { get; set; }
        public Booking? Booking { get; set; }
        public Payment? Payment { get; set; }
    }

    public class WalletReport
    {
        public long WalletId { get; set; }
        public long CustomerId { get; set; }
        public decimal Balance { get; set; }
        public List<WalletTransactionReport> Transactions { get; set; } = new();
    }

    public class WalletTransactionReport
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

    public class AdminWalletTransactionReport : WalletTransactionReport
    {
        public long WalletId { get; set; }
        public long CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhoneNumber { get; set; } = "";
        public decimal WalletBalance { get; set; }
    }
}
