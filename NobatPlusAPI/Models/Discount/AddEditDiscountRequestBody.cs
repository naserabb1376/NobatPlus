using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Discount
{
    public class AddEditDiscountRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string DiscountCode { get; set; }

        [Display(Name = "میزان تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, int.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public int DiscountAmount { get; set; }

        [Display(Name = "تاریخ شروع")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime StartDate { get; set; }

        [Display(Name = "تاریخ پایان")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime EndDate { get; set; }

        public string? Description { get; set; }

    }
}
