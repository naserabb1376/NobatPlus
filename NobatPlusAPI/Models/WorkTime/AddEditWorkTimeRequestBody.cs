using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.WorkTime
{
    public class AddEditWorkTimeRequestBody
    {
        public long ID {  get; set; }

       [Display(Name = "کد خدمات دهنده")]
       [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long StylistID { get; set; }

        [Display(Name = "زمان شروع کار")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public TimeSpan WorkStartTime { get; set; }

        [Display(Name = "زمان پایان کار")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public TimeSpan WorkEndTime { get; set; }

        [Display(Name = "روز هفته")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string DayOfWeek { get; set; }

    }
}

