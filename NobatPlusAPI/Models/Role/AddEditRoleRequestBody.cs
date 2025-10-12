using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Role
{
    public class AddEditRoleRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "نام دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Name { get; set; } 

        [Display(Name = "شرح دسترسی")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Description { get; set; }


    }
}
