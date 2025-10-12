
using Domains;

namespace NobatPlusAPI.ViewModels
{
    // Role: جدول نقش‌ها
    public class RoleVM : BaseEntity
    {
        public string Name { get; set; } // نام نقش (مثلاً Student, Teacher, Admin)
        public string? Description { get; set; } // توضیحات نقش
     
    }
}