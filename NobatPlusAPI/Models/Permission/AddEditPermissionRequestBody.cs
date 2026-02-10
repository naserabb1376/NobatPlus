using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Permission
{
    public class AddEditPermissionRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "نام دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Name { get; set; }

        [Display(Name = "کلید دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Key { get; set; }

        [Display(Name = "آیکون دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Icon { get; set; }

        [Display(Name = "مسیر دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Routename { get; set; }

        [Display(Name = "شرح دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Description { get; set; }

        [Display(Name = "نوع دسترسی")]
        public string? PermissionType { get; set; }

        [Display(Name = "کد منوها")]
        public string? MenuIds { get; set; }

        [Display(Name = "کد منوی والد")]
        public long? MenuParentId { get; set; }


        [Display(Name = "زبان های دیگر")]
        public string? OtherLangs { get; set; } = "";

    }
}