using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Admin
{
    public class AddEditAdminRequestBody
    {
        public long ID { get; set; }

        [Display(Name = "کد شخص")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long PersonId { get; set; }

        [Display(Name = "نقش")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Role { get; set; }

        public string? Description { get; set; }

    }
}

