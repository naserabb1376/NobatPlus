using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Login
{
    public class AddEditLoginRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد شخص")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long PersonID { get; set; }

        [Display(Name = "نام کاربری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Username { get; set; }

        [Display(Name = "کلمه عبور")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Password { get; set; }

        [Display(Name = "تاریخ ورود")]
        public DateTime? LastLoginDate { get; set; }
        public string? Description { get; set; }
    }
}
