using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class SignupRequestBody
    {
        [Display(Name = "نام کاربری")]
        [MaxLength(20)]
        [RegularExpression(@"^[A-Za-z][A-Za-z0-9_]{2,18}$", ErrorMessage = "نام کاربری باید با حروف انگلیسی شروع شود، فقط شامل حروف انگلیسی، اعداد و زیرخط (_) باشد و طول آن بین 4 تا 19 کاراکتر باشد.")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string UserName { get; set; }

        [Display(Name = "رمز عبور")]
        [MaxLength(20)]
        [DataType(DataType.Password)] //Hide Characters
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,20}$", ErrorMessage = "رمز عبور باید شامل حرف و عدد باشد")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Password { get; set; }
        public bool IsStylist { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید ")]
        [MaxLength(200)]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "پست الکترونیک معتبر نیست")]
        [Display(Name = "پست الکترونیک")]
        public string Email { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفا {0} شخص را وارد کنید")]
        [RegularExpression(@"^([0-9]{11})$", ErrorMessage = "مقدار {0} باید 11 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(11)]
        public string PhoneNumber { get; set; }

        [Display(Name = "شهر یا استان")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long CityID { get; set; }

        [Display(Name = "خیابان")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string AddressStreet { get; set; }

        [Display(Name = "کد پستی")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "مقدار {0} باید 10 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(10)]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string AddressPostalCode { get; set; }
        public string? AddressLocationHorizentalPoint { get; set; }
        public string? AddressLocationVerticalPoint { get; set; }

        [Display(Name = "کد ملی")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "مقدار {0} باید 10 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(10)]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string NaCode { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Specialty { get; set; }
        public int YearsOfExperience { get; set; }
        public long StylistParentID { get; set; }

        [Display(Name = "گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long JobTypeID { get; set; }
        public string? PersonDescription { get; set; }
        public string? CustomerDescription { get; set; }
        public string? RegisterDescription { get; set; }
        public string? LoginDescription { get; set; }
        public string? StylistDescription { get; set; }
        public string? AddressDescription { get; set; }
    }
}