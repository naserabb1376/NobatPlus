using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PaymentHistory
{
    public class AddEditPaymentHistoryRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long PaymentID { get; set; }

        [Display(Name = "تاریخ پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "روش پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public bool PaymentStatus { get; set; }
        public string? TransactionCode { get; set; }
        public string? TrackingNumber { get; set; }
        public string? GatewayName { get; set; }
        public string? GatewayMessage { get; set; }
        public string? PaymentToken { get; set; }
        public string? Description { get; set; }
    }
}
