using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class CheckCodeRequestBody
    {
        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفا {0} شخص را وارد کنید")]
        [RegularExpression(@"^([0-9]{11})$", ErrorMessage = "مقدار {0} باید 11 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(11)]
        public string PhoneNumber { get; set; }

        [Display(Name = "کد تایید")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [RegularExpression(@"^([0-9]{6})$", ErrorMessage = "کد ارسال شده نامعتبر است")]
        [MaxLength(6)]
        public string VerifyCode { get; set; }
        public bool Exists { get; set; }


    }
}