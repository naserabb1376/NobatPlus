using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.Public
{
    public class GetRowRequestBody
    {
        [Display(Name = "کد ردیف")]
        [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
        public long ID { get; set; }
    }
}
