using System.ComponentModel.DataAnnotations;
using Domains;

namespace Domain
{
    public class Address : BaseEntity
    {
        [Display(Name = "شهر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [MinLength(50)]
        public string AddressCity { get; set; }

        [Display(Name = "خیابان")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [MinLength(100)]
        public string AddressStreet { get; set; }

        [Display(Name = "کد پستی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [MinLength(10)]
        public string AddressPostalCode { get; set; }

        [Display(Name = "مختصات افقی")]
        [MinLength(500)]
        public string? AddressLocationHorizentalPoint { get; set; }

        [Display(Name = "مختصات عمودی")]
        [MinLength(500)]
        public string? AddressLocationVerticalPoint { get; set; }
    }
}