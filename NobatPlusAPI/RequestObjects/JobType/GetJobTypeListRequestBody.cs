using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.JobType
{
    public class GetJobTypeListRequestBody:GetListRequestBody
    {
        [Display(Name = "جنسیت")]
        public int SexTypeChecked { get; set; } = 0;

    }
}
