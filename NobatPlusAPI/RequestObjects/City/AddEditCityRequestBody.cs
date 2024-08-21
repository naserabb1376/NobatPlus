using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.City
{
    public class AddEditCityRequestBody
    {
        public long ID { get; set; } = 0;

        [Display(Name = "نام شهر")]
        [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
        public string CityName { get; set; }
        public long ParentId { get; set; } = 0; 
        public string? Description { get; set; }

    }
}
