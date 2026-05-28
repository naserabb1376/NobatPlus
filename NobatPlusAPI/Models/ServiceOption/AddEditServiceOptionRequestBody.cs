using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ServiceOption
{
    public class AddEditServiceOptionRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "سرویس")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceManagementID { get; set; }

        [Display(Name = "عنوان گزینه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string OptionName { get; set; }

        [Display(Name = "کلید گزینه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string OptionKey { get; set; }

        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }
}
