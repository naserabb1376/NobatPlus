using Domain;
using NobatPlusAPI.Models.Address;
using NobatPlusAPI.Tools;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Authenticate
{
    public class SignupRequestBody
    {
        //[Display(Name = "نام کاربری")]
        //[MaxLength(20)]
        //[RegularExpression(@"^[A-Za-z][A-Za-z0-9_]{2,18}$", ErrorMessage = "نام کاربری باید با حروف انگلیسی شروع شود، فقط شامل حروف انگلیسی، اعداد و زیرخط (_) باشد و طول آن بین 4 تا 19 کاراکتر باشد.")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        //public string UserName { get; set; }

        //[Display(Name = "رمز عبور")]
        //[MaxLength(20)]
        //[DataType(DataType.Password)] //Hide Characters
        //[RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,20}$", ErrorMessage = "رمز عبور باید شامل حرف و عدد باشد")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        //public string Password { get; set; }

        [Display(Name = "نام")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string LastName { get; set; }

       // [Required(ErrorMessage = "لطفا {0} را وارد کنید ")]
        [MaxLength(200)]
        [RegularExpression(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", ErrorMessage = "پست الکترونیک معتبر نیست")]
        [Display(Name = "پست الکترونیک")]
        public string? Email { get; set; }

        [Display(Name = "شماره موبایل")]
        [Required(ErrorMessage = "لطفا {0} شخص را وارد کنید")]
        [RegularExpression(@"^([0-9]{11})$", ErrorMessage = "مقدار {0} باید 11 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(11)]
        public string PhoneNumber { get; set; }

        [Display(Name = "کد ملی")]
        [RegularExpression(@"^([0-9]{10})$", ErrorMessage = "مقدار {0} باید 10 رقمی و فقط شامل اعداد باشد")]
        [MaxLength(10)]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string NaCode { get; set; }

        [Display(Name = "تاریخ تولد")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string DateOfBirth { get; set; }

        [Display(Name = "جنسیت کاربر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public int Gender { get; set; }

        [Display(Name = "نقش کاربر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public int RoleId { get; set; } // 1. User 2.Stylist 3. Salon
        public string? Specialty { get; set; }

        [RequiredIfRole(nameof(RoleId), 2, 3, ErrorMessage = "لطفاً {0} را وارد کنید")]
        public int YearsOfExperience { get; set; }

        [RequiredIfRole(nameof(RoleId), 2, 3, ErrorMessage = "لطفاً {0} را وارد کنید")]
        public long StylistParentID { get; set; }

        [Display(Name = "گروه شغلی")]
        [RequiredIfRole(nameof(RoleId), 2, 3, ErrorMessage = "لطفاً {0} را وارد کنید")]
        public long JobTypeID { get; set; }
        //public string? PersonDescription { get; set; }
        //public string? CustomerDescription { get; set; }
        //public string? RegisterDescription { get; set; }
        //public string? LoginDescription { get; set; }
        //public string? StylistDescription { get; set; }

        [RequiredIfRole(nameof(RoleId), 2, 3, ErrorMessage = "لطفاً {0} را وارد کنید")]
        public AddEditAddressRequestBody? Address { get; set; }
    }
}