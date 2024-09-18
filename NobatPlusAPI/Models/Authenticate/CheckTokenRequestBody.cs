using NobatPlusAPI.Tools;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class CheckTokenRequestBody
    {
        [Display(Name = "توکن")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Token { get; set; }

        [Display(Name = "نوع توکن")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string TokenType { get; set; }
        public bool TokenStatus { get; set; }
    }
}
