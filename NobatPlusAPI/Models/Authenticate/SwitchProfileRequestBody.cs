using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class SwitchProfileRequestBody
    {
        [Required]
        [RegularExpression("^(stylist|salon)$", ErrorMessage = "نوع پروفایل باید stylist یا salon باشد")]
        public string ProfileType { get; set; } = "";

        [Range(1, long.MaxValue)]
        public long ProfileId { get; set; }
    }
}
