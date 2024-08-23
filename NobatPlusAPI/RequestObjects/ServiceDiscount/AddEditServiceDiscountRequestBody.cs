using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ServiceDiscount
{
    public class AddEditServiceDiscountRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "شناسه تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long DiscountID { get; set; }
        [Display(Name = "کد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceID { get; set; }

        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistID { get; set; }

        [Display(Name = "کد مدیر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long AdminID { get; set; }
        public string? Description { get; set; }
    }
}
