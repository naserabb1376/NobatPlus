using System.ComponentModel.DataAnnotations;
using Domains;
using NobatPlusDATA.Domain;

namespace Domain
{
    public class Address : BaseEntity
    {
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

        public City City { get; set; }
        public long CityID { get; set; }
    }

    public class FindLocationRequestBody
    {
        [Display(Name = "عرض جغرافیایی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public double LocationLatitude { get; set; }

        [Display(Name = "طول جغرافیایی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public double LocationLongitude { get; set; }

        [Display(Name = "شعاع کیلومتری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public double RadiusKm { get; set; }
    }
}