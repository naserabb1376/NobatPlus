using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.StylistService
{
    public class AddEditStylistServiceRequestBody
    {
        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long StylistID { get; set; }

        [Display(Name = "کد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long ServiceID { get; set; }

        [Display(Name = "قیمت خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public decimal ServicePrice { get; set; }

        [Display(Name = "درصد بیعانه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int DepositPercent { get; set; }

        [Display(Name = "مدت خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public TimeSpan Duration { get; set; }
    }
}


