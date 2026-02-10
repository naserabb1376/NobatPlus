using Domains;

namespace NobatPlusAPI.ViewModels
{ // Permission: جدول دسترسی‌ها
    public class PermissionVM : BaseEntity
    {
        public string Key { get; set; } = default!; // users.edit (unique)
        public string Name { get; set; } // نام دسترسی (مثلاً Create, Edit, Delete)
        public string? Icon { get; set; }
        public string? Routename { get; set; }
        public string? PermissionType { get; set; }
        public string? Description { get; set; }
        public string? MenuIds { get; set; }
        public long? MenuParentId { get; set; }

    }
}