using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Register
{
    public class AddEditRegisterRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد شخص")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long PersonID { get; set; }

        [Display(Name = "تاریخ ثبت نام")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime? RegisterDate { get; set; }
        public string? Description { get; set; }
    }
}
