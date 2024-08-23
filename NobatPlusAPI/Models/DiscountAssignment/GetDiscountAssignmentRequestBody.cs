using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.DiscountAssignment
{
    public class GetDiscountAssignmentListRequestBody:GetListRequestBody
    {
        [Display(Name = "شناسه تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long DiscountId { get; set; }

        [Display(Name = "کد مدیر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long AdminId { get; set; }

        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]

        public long StylistId { get; set; }

    }
}
