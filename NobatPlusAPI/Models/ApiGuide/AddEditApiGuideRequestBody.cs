using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ApiGuide
{
    public class AddEditApiGuideRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "عنوان سرویس")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ApiName { get; set; }

        [Display(Name = "نوع راهنما")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string GuideType { get; set; }

        [Display(Name = "عنوان آبجکت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ModelName { get; set; }

        [Display(Name = "نام خصوصیت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FieldEnglishName { get; set; }

        [Display(Name = "نوع داده خصوصیت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FieldDataType { get; set; }

        [Display(Name = "نام فارسی خصوصیت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FieldFarsiName { get; set; }

        [Display(Name = "نوع المنت پیشنهادی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string FieldRecomendedInputType { get; set; }
        public string? Description { get; set; }
    }
}
