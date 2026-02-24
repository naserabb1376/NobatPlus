using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Discount
{
    public class ExistDiscountRequestBody
    {
        [Display(Name = "نوع جستجو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ExistType { get; set; }

        [Display(Name = "مقدار کلید جستجو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string KeyValue { get; set; }
    }
}