using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Discount
{
    public class GetDiscountListRequestBody:GetListRequestBody
    {
        [Display(Name = "نوع تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public int DiscountType { get; set; }

        [Display(Name = "شناسه تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long DiscountId { get; set; }

        [Display(Name = "کد مدیر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long AdminId { get; set; }

        [Display(Name = "کد مشتری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CustomerId { get; set; }

        [Display(Name = "کد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceId { get; set; }

        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistId { get; set; }

    }
}
