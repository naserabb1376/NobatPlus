using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Wallet
{
    public class GetWalletRequestBody
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetCustomerWalletRequestBody : GetWalletRequestBody
    {
        [Range(1, long.MaxValue, ErrorMessage = "شناسه مشتری معتبر نیست")]
        public long CustomerId { get; set; }
    }

    public class GetWalletTransactionsRequestBody
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchText { get; set; }
        public string? TransactionType { get; set; }
    }

    public class ChargeWalletRequestBody
    {
        [Range(1, double.MaxValue, ErrorMessage = "مبلغ شارژ کیف پول معتبر نیست")]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class ChargeCustomerWalletRequestBody : ChargeWalletRequestBody
    {
        [Range(1, long.MaxValue, ErrorMessage = "شناسه مشتری معتبر نیست")]
        public long CustomerId { get; set; }
    }

    public class PayBookingWithWalletRequestBody
    {
        [Range(1, long.MaxValue, ErrorMessage = "شناسه رزرو معتبر نیست")]
        public long BookingID { get; set; }
        public long? DiscountID { get; set; }
        public string? Description { get; set; }
    }
}
