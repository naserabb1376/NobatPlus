using Domains;
using System.ComponentModel.DataAnnotations.Schema;

namespace NobatPlusDATA.Domain
{
    // Role: جدول نقش‌ها

    //[NotMapped]   // ⛔ در دیتابیس ساخته نمی‌شود
    public class Role : BaseEntity
    {
        public string Name { get; set; } // نام نقش (مثلاً Student, Teacher, Admin)
        public string? Description { get; set; } // توضیحات نقش
        //public ICollection<Person> Persons { get; set; } // کاربران مرتبط با نقش
       // public ICollection<PermissionRole> PermissionRoles { get; set; } // دسترسی‌های مرتبط با نقش
     
        // public ICollection<Permission> Permissions { get; set; } // دسترسی‌های مرتبط با نقش
    }
}