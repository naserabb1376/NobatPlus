using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Setting
{
    public class GetSettingRowRequestBody
    {
        [Display(Name = "کد")]
        public long ID { get; set; } = 0;

        [Display(Name = "کلید")]
        public string Key { get; set; } = "";

        //[Display(Name = "زبان")]
        //public string? Lang { get; set; } = "";

    }
}
