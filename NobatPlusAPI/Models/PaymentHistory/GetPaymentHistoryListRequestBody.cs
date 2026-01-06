using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PaymentHistory
{
    public class GetPaymentHistoryListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد رزرو")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long BookingId { get; set; } = 0;

        [Display(Name = "کد پرداخت")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long PaymentId { get; set; } = 0;

    }
}
