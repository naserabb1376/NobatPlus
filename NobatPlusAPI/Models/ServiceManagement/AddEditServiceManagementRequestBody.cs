using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.ServiceManagement
{
    public class AddEditServiceManagementRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "کد والد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long ServiceParentID { get; set; }

        [Display(Name = "عنوان خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string ServiceName { get; set; }

        [Display(Name = "مدت خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public TimeSpan Duration { get; set; }

        [Display(Name = "قیمت خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long Price { get; set; }
        public string? Description { get; set; }
    }
}
