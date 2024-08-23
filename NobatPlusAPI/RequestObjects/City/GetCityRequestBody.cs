using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.City
{
    public class GetCityListRequestBody:GetListRequestBody
    {
        [Display(Name = "کد والد")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ParentId { get; set; } = -1;

    }
}
