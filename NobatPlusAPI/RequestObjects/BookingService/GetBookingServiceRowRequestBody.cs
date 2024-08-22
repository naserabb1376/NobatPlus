using Domain;
using NobatPlusAPI.RequestObjects.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class GetBookingServiceRowRequestBody
    {
        [Display(Name = "کد رزرو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long BookingID { get; set; }

        [Display(Name = "کد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceID { get; set; }
    }
}


