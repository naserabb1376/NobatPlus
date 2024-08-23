using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Stylist
{
    public class AddEditStylistRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد والد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistParentID { get; set; }

        [Display(Name = "کد شخص")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long PersonID { get; set; }

        [Display(Name = "زمینه تخصص")]
        public string? Specialty { get; set; }

        [Display(Name = "میزان تجربه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int YearsOfExperience { get; set; }

        [Display(Name = "کد گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long JobTypeID { get; set; }
        public string? Description { get; set; }
    }
}
