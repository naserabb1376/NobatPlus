using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class RefreshTokenRequestBody
    {
        [Display(Name = "رفرش توکن")]
        public string? RefreshToken { get; set; }
    }
}
