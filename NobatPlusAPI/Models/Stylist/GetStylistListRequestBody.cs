using Domain;
using NobatPlusAPI.Models.Address;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Stylist
{
    public class GetStylistListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد والد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ParentID { get; set; }

        [Display(Name = "کد شهر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CityID { get; set; }

        [Display(Name = "شناسه تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long DiscountID { get; set; }

        [Display(Name = "کد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceID { get; set; }

        [Display(Name = "کد گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long JobTypeID { get; set; }

        public FindLocationRequestBody? FindLocationRequestBody { get; set; }
    }
}
