using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Payment
{
    public class RequestPaymentRequestBody
    {
        [Display(Name = "کد پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long PaymentID { get; set; }
    }

    public class RequestPaymentResultBody
    {
        public string? PayGatewayUrl { get; set; }
        public long PaymentHistoryID { get; set; }
        public decimal Amount { get; set; }
    }
}
