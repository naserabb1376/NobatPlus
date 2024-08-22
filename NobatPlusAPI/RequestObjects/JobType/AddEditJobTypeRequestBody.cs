using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.JobType
{
    public class AddEditJobTypeRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "عنوان گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string JobTitle { get; set; }
        public int SexTypeChecked { get; set; } = 0;
        public string? Description { get; set; }
    }
}
