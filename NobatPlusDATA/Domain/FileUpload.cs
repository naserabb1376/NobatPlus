using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    // FileUpload: جدول فایل‌های ارسال‌شده
    public class FileUpload : BaseEntity
    {
        public string FileName { get; set; } // نام فایل
        public string FilePath { get; set; } // مسیر فایل
        public string ContentType { get; set; } // نوع فایل (مثلاً PDF, JPEG)
        public string Description { get; set; } = ""; // توضیحات فایل
        public long CreatorId { get; set; } = 0; // کاربر ایجاد کننده
        public long ForeignKeyId { get; set; } // کلید خارجی به رکورد اصلی
        public string EntityType { get; set; } // نوع جدول مرتبط (مثلاً "User", "Course", "Event")
        public string? GetUrl { get; set; } // لینک دانلود
        public long? FileNumber { get; set; } //کد فایل
        public string ReviewStatus { get; set; } = "pending";
        public long? ReviewedByPersonID { get; set; }
        public Person? ReviewedByPerson { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }

    }
}
