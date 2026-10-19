using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.JobType
{
    public class GetJobTypeListRequestBody:GetListRequestBody
    {
        [Display(Name = "نقش کاربر")]
        public long RoleId { get; set; } = 0;

        [Display(Name = "کد والد نوع شغل")]
        public long ParentID { get; set; } = 0;

        [Display(Name = "جنسیت")]
        public int SexTypeChecked { get; set; } = 0;

    }
}
