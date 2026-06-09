using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.FinancialAccount
{
    public class GetFinancialAccountRequestBody
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TransactionType { get; set; }
    }

    public class UpdateFinancialBankInfoRequestBody
    {
        public string? Iban { get; set; }
        public string? BankAccountOwnerName { get; set; }
    }

    public class RequestSettlementRequestBody
    {
        [Range(1, double.MaxValue, ErrorMessage = "مبلغ تسویه معتبر نیست")]
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class GetSettlementRequestsRequestBody
    {
        public string? Status { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchText { get; set; }
    }

    public class UpdateSettlementStatusRequestBody
    {
        [Range(1, long.MaxValue, ErrorMessage = "شناسه درخواست تسویه معتبر نیست")]
        public long SettlementRequestID { get; set; }
        public string Status { get; set; } = "";
        public string? TrackingCode { get; set; }
        public string? RejectReason { get; set; }
    }
}
