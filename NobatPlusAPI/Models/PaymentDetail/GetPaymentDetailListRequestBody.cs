using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PaymentDetail
{
    public class GetPaymentDetailListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد خدمت آرایشگر")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistServiceId { get; set; } = 0;

        [Display(Name = "کد پرداخت")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long PaymentId { get; set; } = 0;

    }
}
