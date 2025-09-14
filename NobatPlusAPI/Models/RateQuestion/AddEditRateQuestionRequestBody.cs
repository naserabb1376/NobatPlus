using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.RateQuestion
{
    public class AddEditRateQuestionRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "متن سوال")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string RateQuestionText { get; set; }
        public string? Description { get; set; }
    }
}
