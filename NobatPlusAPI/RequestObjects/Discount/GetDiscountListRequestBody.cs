using Domain;
using NobatPlusAPI.RequestObjects.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class GetDiscountListRequestBody:GetListRequestBody
    {
        [Display(Name = "نوع تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
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
