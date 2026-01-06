using Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    // Setting: جدول تنظیمات اصلی سایت
    public class Setting : BaseEntity
    {
        public string Key { get; set; } // کلید تنظیمات
        public string Value { get; set; } // مقدار تنظیمات
        public long? ParentId { get; set; } // کلید والد برای تنظیمات درختی
        public Setting? Parent { get; set; } // تنظیمات والد
        public ICollection<Setting> Children { get; set; } // تنظیمات فرزند
    }
}