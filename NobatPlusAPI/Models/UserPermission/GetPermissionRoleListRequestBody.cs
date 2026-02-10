using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.UserPermission
{
    public class GetUserPermissionListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد کاربر")]
        public long UserId { get; set; } = 0;

        [Display(Name = "کد دسترسی")]
        public long PermissionId { get; set; } = 0;

        [Display(Name = "نوع دسترسی")]
        public string? PermissionType { get; set; } = "";

    }
}
