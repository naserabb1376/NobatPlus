using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PaymentHistory
{
    public class AddEditPaymentHistoryRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد رزرو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long BookingID { get; set; }

        [Display(Name = "مبلغ پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long Amount { get; set; }

        [Display(Name = "تاریخ پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "روش پرداخت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string PaymentMethod { get; set; }
        public string? Description { get; set; }
    }
}
