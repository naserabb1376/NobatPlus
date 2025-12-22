using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Review
{
    public class AddEditReviewRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد رزرو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long BookingID { get; set; }

        [Display(Name = "کد مشتری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long CustomerID { get; set; }

        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long StylistID { get; set; }

        [Display(Name = "امتیاز")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int Rating { get; set; }

        [Display(Name = "متن نظر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Comments { get; set; }

        [Display(Name = "وضعیت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Status { get; set; }

        [Display(Name = "تعداد لایک")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int LikeCount { get; set; }

        [Display(Name = "تعداد دیسلایک")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int DislikeCount { get; set; }

        [Display(Name = "تاریخ ثبت نظر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime? ReviewDate { get; set; }

        public string? Description { get; set; }

        [Display(Name = "سطح نمایش")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public bool IsPrivate { get; set; }

    }
}
