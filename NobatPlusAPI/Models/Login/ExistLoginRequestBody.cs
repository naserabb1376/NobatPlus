using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Login
{
    public class ExistLoginRequestBody
    {
        [Display(Name = "کلید جستجو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string UniqueProperty { get; set; }

        [Display(Name = "روش جستجو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int SearchMode { get; set; } = 1;

    }
}
