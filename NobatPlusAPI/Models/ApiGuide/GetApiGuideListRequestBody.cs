using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ApiGuide
{
    public class GetApiGuideListRequestBody : GetListRequestBody
    {
        [Display(Name = "نوع راهنما")]
        public string GuideType { get; set; } = "";

    }
}
