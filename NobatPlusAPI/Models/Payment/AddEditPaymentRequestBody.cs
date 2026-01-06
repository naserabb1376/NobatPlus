using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Payment
{
    public class AddEditPaymentRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد رزرو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long BookingID { get; set; }

        [Display(Name = "تاریخ پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "وضعیت پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string PaymentStatus { get; set; }
        public string? Description { get; set; }
    }
}
