using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.StylistPacific
{
    public class AddEditStylistPacificRequestBody
    {
        public long ID {  get; set; }

       [Display(Name = "کد خدمات دهنده")]
       [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long StylistID { get; set; }

        [Display(Name = "تاریخ شروع مرخصی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime PacificStartDate { get; set; }

        [Display(Name = "تاریخ پایان مرخصی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime PacificEndDate { get; set; }

    }
}

