using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PaymentDetail
{
    public class GetPaymentDetailListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد آرایشگر")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistId { get; set; } = 0;

        [Display(Name = "کد خدمت")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceId { get; set; } = 0;

        [Display(Name = "کد پرداخت")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long PaymentId { get; set; } = 0;

    }
}
