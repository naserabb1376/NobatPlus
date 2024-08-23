using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Address
{
    public class GetAddressListRequestBody:GetListRequestBody
    {
        [Display(Name = "کد شهر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CityId { get; set; } = 0;
    }
}
