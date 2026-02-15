using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Permission
{
    public class GetPermissionListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد نقش")]
        public long? RoleId { get; set; }

        [Display(Name = "کد کاربر")]
        public long? UserId { get; set; }

        [Display(Name = "نوع دسترسی")]
        public string? PermissionType { get; set; } = "";

        [Display(Name = "کد منوی والد")]
        public long MenuParentId { get; set; } = 0;

        [Display(Name = "کد منوها")]
        public string MenuIds { get; set; } = "";
    }
}
