using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Review
{
    public class GetReviewListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد رزرو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long BookingId { get; set; } = 0;

        [Display(Name = "کد مشتری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CustomerId { get; set; } = 0;
    }
}
