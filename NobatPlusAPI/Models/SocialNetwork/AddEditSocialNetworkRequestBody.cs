using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.SocialNetwork
{
    public class AddEditSocialNetworkRequestBody
    {
        public long ID {  get; set; }

       [Display(Name = "کد خدمات دهنده")]
       [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long StylistID { get; set; }

        [Display(Name = "نام شبکه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string SocialNetworkName { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string PhoneNumber { get; set; }

        [Display(Name = "لینک حساب")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string AccountLink { get; set; }

        [Display(Name = "آیکن شبکه")]
        public string? SocialNetworkIcon { get; set; }

    }
}

