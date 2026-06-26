using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Person
{
    public class GetPersonListRequestBody : GetListRequestBody
    {

        [Display(Name = "کد شهر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CityId { get; set; } = 0;

        [Display(Name = "نقش کاربر")]
        public long RoleId { get; set; } = 0;

        [Display(Name = "وضعیت فعال بودن")]
        public bool? IsActive { get; set; } = null;
    }
}
