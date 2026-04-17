using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Payment
{
    public class GetPaymentListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد مشتری")]
        // [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long? CustomerId { get; set; }

        [Display(Name = "کد رزرو")]
       // [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long BookingId { get; set; } = 0;

        [Display(Name = "وضعیت پرداخت")]
        // [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int PaymentIncludes { get; set; } = 0;
    }
}
