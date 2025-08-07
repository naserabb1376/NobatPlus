using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.SMSMessage
{
    public class GetSMSMessageListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد شخص")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long PersonId { get; set; } = 0;
    }
}
