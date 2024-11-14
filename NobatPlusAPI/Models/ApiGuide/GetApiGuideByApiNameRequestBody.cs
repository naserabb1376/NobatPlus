using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ApiGuide
{
    public class GetApiGuideByApiNameRequestBody
    {
        [Display(Name = "عنوان سرویس")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ApiName { get; set; }

        [Display(Name = "نوع راهنما")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string GuideType { get; set; }

    }
}
