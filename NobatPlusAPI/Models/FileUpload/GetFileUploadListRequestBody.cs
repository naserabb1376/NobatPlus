using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.FileUpload
{
    public class GetFileUploadListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد فایل")]
        public long ForeignKeyId { get; set; } = 0;

        [Display(Name = "نام شی")]
        public string entityType { get; set; }

        [Display(Name = "کاربر ایجاد کننده")]
        public long CreatorId { get; set; } = 0;
    }
}