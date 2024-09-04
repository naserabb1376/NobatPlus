using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class SendCodeRequestBody
    {
        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفا {0} شخص را وارد کنید")]
        [RegularExpression(@"^([0-9]{11})$", ErrorMessage = "مقدار {0} باید 11 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(11)]
        public string PhoneNumber { get; set; }
        public bool Exists { get; set; }

    }
}