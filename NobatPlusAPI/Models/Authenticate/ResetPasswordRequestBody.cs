using NobatPlusAPI.Tools;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class ResetPasswordRequestBody
    {
        [Display(Name = "توکن")]
        public string Token { get; set; }

        [Display(Name = "شماره موبایل")]
        [RegularExpression(@"^([0-9]{11})$", ErrorMessage = "مقدار {0} باید 11 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(11)]
        public string PhoneNumber { get; set; }

        [Display(Name = "کد تایید")]
        [MaxLength(6)]
        public string VerifyCode { get; set; }

        [Display(Name = "کلمه عبور جدید")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [MaxLength(20)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,20}$", ErrorMessage = "کلمه عبور باید شامل حرف و عدد باشد")] //check exist number & alphabet chars in password field
        public string NewPassword { get; set; }

        [Display(Name = "تکرار کلمه عبور جدید")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Compare("NewPassword", ErrorMessage = "کلمه عبور و تکرار کلمه عبور یکسان نیستند")] //Compare value with Password field
        [MaxLength(20)]
        public string ReNewPassword { get; set; }
    }
}
