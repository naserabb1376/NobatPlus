using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Log
{
    public class AddEditLogRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "نام اکشن")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ActionName { get; set; }

        [Display(Name = "تاریخ لاگ")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime LogTime { get; set; }
        public string? Description { get; set; }
    }
}
