using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.UserPermission
{
    public class AddEditUserPermissionRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد کاربر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long UserId { get; set; }

        [Display(Name = "کد دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long PermissionId { get; set; }
        public bool IsGranted { get; set; } = true; // allow/deny override
        public bool OwnerOnly { get; set; }

    }
}
