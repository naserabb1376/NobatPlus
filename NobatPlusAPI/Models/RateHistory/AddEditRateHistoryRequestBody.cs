using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.RateHistory
{
    public class AddEditRateHistoryRequestBody
    {
        public long ID { get; set; }

        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long StylistID { get; set; }

        [Display(Name = "کد مشتری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long CustomerID { get; set; }

        [Display(Name = "کد سوال")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long RateQuestionID { get; set; }

        [Display(Name = "امتیاز")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, float.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public float RateScore { get; set; }

        [Display(Name = "تاریخ امتیاز دهی")]
        public string? RateDate { get; set; }
        public string? Description { get; set; } = "";


    }
}
