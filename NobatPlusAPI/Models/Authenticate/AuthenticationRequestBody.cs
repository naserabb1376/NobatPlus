using NobatPlusAPI.Tools;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class AuthenticationRequestBody
    {
        [Display(Name = "نام کاربری")]
        [MaxLength(20)]
        [ConditionalRegularExpression("LoginType", @"^[A-Za-z][A-Za-z0-9_]{2,18}$", ErrorMessage = "نام کاربری باید با حروف انگلیسی شروع شود، فقط شامل حروف انگلیسی، اعداد و زیرخط (_) باشد و طول آن بین 4 تا 19 کاراکتر باشد.")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string UserName { get; set; }

        [Display(Name = "رمز عبور")]
        [MaxLength(20)]
        [ConditionalRegularExpression("LoginType", @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,20}$", ErrorMessage = "رمز عبور باید شامل حرف و عدد باشد")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Password { get; set; }
        public string CaptchaCode { get; set; }
        public int LoginType { get; set; } = 1;
    }
}
