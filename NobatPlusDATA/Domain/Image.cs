using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    // Image: جدول تصاویر
    public class Image : BaseEntity
    {
        public string FileName { get; set; } // نام فایل
        public string FilePath { get; set; } // مسیر فایل
        public long ForeignKeyId { get; set; } // کلید خارجی به رکورد اصلی
        public string EntityType { get; set; } // نوع جدول مرتبط (مثلاً "User", "Course", "Event")
        public string Description { get; set; } = ""; // توضیحات تصویر
        public long CreatorId { get; set; } = 0; // کاربر ایجاد کننده
        public int? Priority { get; set; }
    }
}