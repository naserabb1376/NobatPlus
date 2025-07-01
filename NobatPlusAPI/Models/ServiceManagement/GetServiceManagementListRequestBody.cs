using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ServiceManagement
{
    public class GetServiceManagementListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد والد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ParentID { get; set; }

        [Display(Name = "کد رزرو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long BookingID { get; set; }

        [Display(Name = "شناسه تخفیف")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long DiscountID { get; set; }

        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistID { get; set; }

        [Display(Name = "جنسیت خدمات")]
        public char? ServiceGender { get; set; }
    }
}
