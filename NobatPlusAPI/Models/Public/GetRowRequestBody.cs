using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Public
{
    public class GetRowRequestBody
    {
        [Display(Name = "کد ردیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long ID { get; set; }
    }
}
