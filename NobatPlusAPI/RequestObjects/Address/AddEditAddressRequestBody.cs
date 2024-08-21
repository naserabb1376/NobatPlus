using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class AddEditAddressRequestBody
    {
        [Display(Name = "شهر")]
        [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
        public long CityID { get; set; }

        [Display(Name = "خیابان")]
        [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
        public string AddressStreet { get; set; }

        [Display(Name = "کد پستی")]
        [RegularExpression(@"^([0-9]{11})$", ErrorMessage = "مقدار {0} باید 10 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(10)]
        [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
        public string AddressPostalCode { get; set; }
        public string? AddressLocationHorizentalPoint { get; set; }
        public string? AddressLocationVerticalPoint { get; set; }
        public string? AddressDescription { get; set; }
    }
}

