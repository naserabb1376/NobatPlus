using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Setting
{
    public class AddEditSettingRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کلید")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Key { get; set; } 

        [Display(Name = "مقدار")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Value { get; set; } 

        [Display(Name = "کد والد")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        //[Range(0, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long? ParentId { get; set; }

        //[Display(Name = "زبان های دیگر")]
        //public string? OtherLangs { get; set; } = "";

    }
}
