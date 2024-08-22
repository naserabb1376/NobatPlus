using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.City
{
    public class AddEditCustomerRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد شخص")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long PersonID { get; set; }
        public string? Description { get; set; }

    }
}
