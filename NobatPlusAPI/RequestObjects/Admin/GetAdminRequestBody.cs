using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Admin
{
    public class GetAdminListRequestBody:GetListRequestBody
    {
        [Display(Name = "نقش")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Role { get; set; }

        [Display(Name = "شناسه تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long DiscountId { get; set; }

        [Display(Name = "کد شهر یا استان")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CityId { get; set; }
    }
}
