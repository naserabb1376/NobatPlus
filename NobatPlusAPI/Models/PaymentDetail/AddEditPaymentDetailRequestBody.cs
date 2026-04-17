using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PaymentDetail
{
    public class AddEditPaymentDetailRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد آرایشگر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long StylistID { get; set; }

        [Display(Name = "کد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long ServiceManagemntID { get; set; }

        [Display(Name = "کد پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long PaymentID { get; set; }

        [Display(Name = "هزینه خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public decimal StylistServiceAmount { get; set; }

        [Display(Name = "هزینه با تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "درصد تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int DiscountPercent { get; set; }

        public string? Description { get; set; }
    }
}
