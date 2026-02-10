using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PermissionRole
{
    public class AddEditPermissionRoleRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد نقش")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long RoleId { get; set; }
        [Display(Name = "کد دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long PermissionId { get; set; }
        public bool OwnerOnly { get; set; }

    }
}
