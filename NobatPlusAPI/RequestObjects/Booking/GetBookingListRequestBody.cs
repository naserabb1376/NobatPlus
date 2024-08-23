using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Booking
{
    public class GetBookingListRequestBody:GetListRequestBody
    {
        [Display(Name = "کد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceId { get; set; } = 0;
        public int CancelState { get; set; } = 0;
    }

}
