using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class SwitchProfileRequestBody
    {
        [Required]
        [RegularExpression("(?i)^(worker|workshop)$", ErrorMessage = "نوع پروفایل باید worker یا workshop باشد")]
        public string ProfileType { get; set; } = "";

        [Range(1, long.MaxValue)]
        public long ProfileId { get; set; }
    }
}
