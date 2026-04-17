using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Discount
{
    public class AddEditDiscountRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد تخفیف")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string? DiscountCode { get; set; }

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

        [Display(Name = "اعمال با کد تخفیف")]
        public bool CodeRequired { get; set; } = false;

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }

        [Display(Name = "کد مدیر")]
        public long? AdminId { get; set; }

        [Display(Name = "کد ارائه دهنده خدمات")]
        public long? StylistId { get; set; }

        [Display(Name = "کد مشتریان")]
        public List<long> CustomerIds { get; set; } = new List<long>();

        [Display(Name = "کد خدمات")]
        public List<long> ServiceIds { get; set; } = new List<long>();

    }
}
