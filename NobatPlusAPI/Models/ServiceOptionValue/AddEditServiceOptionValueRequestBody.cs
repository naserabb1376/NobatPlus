using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ServiceOptionValue
{
    public class AddEditServiceOptionValueRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "گزینه سرویس")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceOptionID { get; set; }

        [Display(Name = "مقدار گزینه")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ValueName { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
    }
}
