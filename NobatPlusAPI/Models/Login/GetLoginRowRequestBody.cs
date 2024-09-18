using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Login
{
    public class GetLoginRowRequestBody : GetRowRequestBody
    {
        [Display(Name = "نوع جستجو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public int SearchMode { get; set; } = 1;

    }
}
