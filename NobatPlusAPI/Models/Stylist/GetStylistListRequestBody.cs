using Domain;
using NobatPlusAPI.Models.Address;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Stylist
{
    public class GetStylistListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد والد خدمات دهنده")]
        //      [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ParentID { get; set; } = 0;

        [Display(Name = "از قیمت")]
        //      [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public decimal FromPrice { get; set; } = 0;

        [Display(Name = "تا قیمت")]
        //      [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public decimal ToPrice { get; set; } = 0;

        [Display(Name = "کد شهر")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CityID { get; set; } = 0;

        [Display(Name = "شناسه تخفیف")]
        //   [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long DiscountID { get; set; } = 0;

        [Display(Name = "لیست کد خدمت")]
        //    [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public List<long> ServiceIDs { get; set; } = new List<long>();

        [Display(Name = "کد گروه شغلی")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long JobTypeID { get; set; } = 0;

        [Display(Name = "جنسیت")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int Gender { get; set; } = 0;


        public FindLocationRequestBody? FindLocationRequestBody { get; set; }
    }
}
