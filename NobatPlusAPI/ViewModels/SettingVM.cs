using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusAPI.ViewModels
{
    // Setting: جدول تنظیمات اصلی سایت
    public class SettingVM : BaseEntity
    {
        public string Key { get; set; } // کلید تنظیمات
        public string Value { get; set; } // مقدار تنظیمات
        public long? ParentId { get; set; } // کلید والد برای تنظیمات درختی
    }
}