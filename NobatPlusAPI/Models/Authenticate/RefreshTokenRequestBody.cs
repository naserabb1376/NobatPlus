using NobatPlusAPI.Tools;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class RefreshTokenRequestBody
    {
        [Display(Name = "رفرش توکن")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string RefreshToken { get; set; }
    }
}
