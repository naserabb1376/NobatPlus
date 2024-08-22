using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.City
{
    public class AddEditCheckAvailabilityRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistID { get; set; }


        [Display(Name = "تاریخ")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime Date { get; set; }


        [Display(Name = "تاریخ")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public TimeSpan Time { get; set; }

        public string? Description { get; set; }

    }
}
