using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class CheckCaptchaCodeRequestBody
    {
        [Display(Name = "کد کپچا")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string CaptchaCode { get; set; }
    }
}