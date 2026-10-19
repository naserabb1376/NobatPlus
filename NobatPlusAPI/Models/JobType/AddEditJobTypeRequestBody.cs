using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.JobType
{
    public class AddEditJobTypeRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "عنوان گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string JobTitle { get; set; }

        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long JobTypeParentID { get; set; } = 0;

        [Display(Name = "نقش کاربر")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long RoleId { get; set; } // 1.User 2.Worker 3.WorkShop 4.Admin

        public int SexTypeChecked { get; set; } = 0;
        public string? Description { get; set; }
    }
}
