
using Domains;

namespace NobatPlusAPI.ViewModels
{ // Permission: جدول دسترسی‌ها
    public class PermissionRoleVM : BaseEntity
    {
        public long RoleId { get; set; }
        public long PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string RouteName { get; set; }
        public string PermissionType { get; set; }
        public bool OwnerOnly { get; set; }

    }
}
