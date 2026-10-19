using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.WorkerService
{
    public class GetWorkerServiceRowRequestBody
    {
        [Display(Name = "کد خدمات دهنده")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long WorkerID { get; set; }

        [Display(Name = "کد خدمت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long ServiceID { get; set; }

        [Display(Name = "کد مشتری")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        //[Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long? CustomerID { get; set; }

        [Display(Name = "شناسه تخفیف")]
        //[Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        //[Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long? DiscountID { get; set; }

        public List<long> OptionValueIDs { get; set; } = new List<long>();
    }
}


