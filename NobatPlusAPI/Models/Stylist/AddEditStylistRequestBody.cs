using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Stylist
{
    public class AddEditStylistRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد والد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistParentID { get; set; }

        [Display(Name = "کد شخص")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long PersonID { get; set; }

        [Display(Name = "زمینه تخصص")]
        public string? Specialty { get; set; }

        [Display(Name = "میزان تجربه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int YearsOfExperience { get; set; }

        [Display(Name = "نام آرایشگر")]
        public string? StylistName { get; set; }

        [Display(Name = "درباره آرایشگر")]
        public string? StylistBio { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string? GenderAccepted { get; set; }

        [Display(Name = "روش کار در محل")]
        public string? WorkShopInteractMode { get; set; }

        [Display(Name = "وضعیت حساب")]
        public string? AccountStatus { get; set; }

        [Display(Name = "روش پرداخت")]
        public string? PayMethod { get; set; }

        [Display(Name = "مقدار اجاره")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long WorkShopRentAmount { get; set; }

        [Display(Name = "مقدار بیعانه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long WorkShopDepositAmount { get; set; }

        [Display(Name = "کد گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long JobTypeID { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
        public bool IsWorkshop { get; set; }
    }

    public class SignupStylistRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد والد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistParentID { get; set; }

        [Display(Name = "زمینه تخصص")]
        public string? Specialty { get; set; }

        [Display(Name = "میزان تجربه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int YearsOfExperience { get; set; }

        [Display(Name = "نام آرایشگر")]
        public string? StylistName { get; set; }

        [Display(Name = "درباره آرایشگر")]
        public string? StylistBio { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string? GenderAccepted { get; set; }

        [Display(Name = "روش کار در محل")]
        public string? WorkShopInteractMode { get; set; }

        [Display(Name = "وضعیت حساب")]
        public string? AccountStatus { get; set; }

        [Display(Name = "روش پرداخت")]
        public string? PayMethod { get; set; }

        [Display(Name = "مقدار اجاره")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long WorkShopRentAmount { get; set; }

        [Display(Name = "مقدار بیعانه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long WorkShopDepositAmount { get; set; }

        [Display(Name = "کد گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long JobTypeID { get; set; }

        [Display(Name = "توضیحات")]
        public string? Description { get; set; }
        public bool IsWorkshop { get; set; }
    }
}
