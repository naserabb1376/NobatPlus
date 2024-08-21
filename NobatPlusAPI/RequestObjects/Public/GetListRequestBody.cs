using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.Public
{
    public class GetListRequestBody
    {
        public int PageIndex { get; set; }

        [Display(Name = "اندازه صفحه")]
        [Required(ErrorMessage = "لطفا {0} را انتخاب کنید")]
        public int PageSize { get; set; }
        public string SearchText { get; set; }
    }
}
