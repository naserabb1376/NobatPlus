using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Setting
{
    public class GetSettingListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد والد")]
        public long ParentId { get; set; } = 0;

    }
}
