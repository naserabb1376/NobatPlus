using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.SMSMessage
{
    public class AddEditSMSMessageRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفا {0} شخص را وارد کنید")]
        [RegularExpression(@"^([0-9]{11})$", ErrorMessage = "مقدار {0} باید 11 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(11)]
        public string PhoneNumber { get; set; }
        [Display(Name = "متن اعلان")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Message { get; set; }

        [Display(Name = "تاریخ ارسال")]
        public string? SentDate { get; set; }
        public string? Description { get; set; }

        public bool PhoneNumberExists { get; set; }
    }
}
