using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.FileUpload
{
    public class ReviewFileUploadRequestBody
    {
        [Display(Name = "شناسه فایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long ID { get; set; }

        [Display(Name = "وضعیت بررسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [RegularExpression("^(pending|approved|rejected)$", ErrorMessage = "وضعیت بررسی معتبر نیست")]
        public string ReviewStatus { get; set; } = "pending";

        [Display(Name = "یادداشت بررسی")]
        public string? ReviewNote { get; set; }
    }
}
