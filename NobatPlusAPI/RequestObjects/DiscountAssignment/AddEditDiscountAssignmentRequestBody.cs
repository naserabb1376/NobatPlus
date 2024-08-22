using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class AddEditDiscountAssignmentRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "شناسه تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long DiscountId { get; set; }

        [Display(Name = "کد مدیر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long AdminId { get; set; }

        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistId { get; set; }
        public string? Description { get; set; }

    }
}
