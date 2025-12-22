using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Booking
{
    public class GetBookingListRequestBody:GetListRequestBody
    {
        [Display(Name = "کد خدمت")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceId { get; set; } = 0;

        [Display(Name = "کد مشتری")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CustomerId { get; set; } = 0;

        [Display(Name = "کد خدمات دهنده")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistId { get; set; } = 0;
        public int CancelState { get; set; } = 0;

        [Display(Name = "از تاریخ")]
        public DateTime? FromDate { get; set; } = null;

        [Display(Name = "تا تاریخ")]
        public DateTime? ToDate { get; set; } = null;
    }

}
