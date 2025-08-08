using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.FileCenter
{
    public class GetFileCenterDownloadListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد رکورد مرتبط با فایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ForeignKeyId { get; set; } = 0;

        [Display(Name = "نام شی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string entityType { get; set; }

        [Display(Name = "نوع فایل")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string fileType { get; set; }


    }
}