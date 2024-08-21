using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.JobType
{
    public class AddEditJobTypeRequestBody
    {
        [Display(Name = "کد گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
        public long ID { get; set; }

        [Display(Name = "عنوان گروه شغلی")]
        [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
        public string JobTitle { get; set; }
        public int SexTypeChecked { get; set; }
        public string? Description { get; set; }

    }
}
