using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Stylist
{
    public class ExistStylistRequestBody
    {
        [Display(Name = "مقدار جستجو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FieldValue { get; set; }

        [Display(Name = "ستون جستجو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FieldName { get; set; }

    }
}
