using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Image
{
    public class AddEditImageRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "نام تصویر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FileName { get; set; } // نام تصویر

        [Display(Name = "مسیر تصویر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FilePath { get; set; }

        [Display(Name = "شرح تصویر")]
        public string? Description { get; set; }

        [Display(Name = "کاربر ایجاد کننده")]
        public long? CreatorId { get; set; }

        [Display(Name = "کلید خارجی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long ForeignKeyId { get; set; }

        [Display(Name = "نام جدول")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string EntityType { get; set; }

        [Display(Name = "اولویت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public int? Priority { get; set; }

        [Display(Name = "لینک دانلود")]
        public string? GetUrl { get; set; } // لینک دانلود
    }
}