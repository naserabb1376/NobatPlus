using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.FileUpload
{
    public class AddEditFileUploadRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "نام فایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FileName { get; set; } // نام فایل

        [Display(Name = "مسیر فایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FilePath { get; set; }

        [Display(Name = "نوع فایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ContentType { get; set; }

        //[Display(Name = "کد تمرین")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        //[Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        //public long AssignmentId { get; set; }
        [Display(Name = "کلید خارجی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long ForeignKeyId { get; set; }

        [Display(Name = "نام جدول")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string EntityType { get; set; }

        [Display(Name = "شرح تصویر")]
        public string? Description { get; set; }

        [Display(Name = "کاربر ایجاد کننده")]
        public long? CreatorId { get; set; }
    }
}