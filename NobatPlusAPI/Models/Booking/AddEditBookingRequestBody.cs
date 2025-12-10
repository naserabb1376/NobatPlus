using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Booking
{
    public class AddEditBookingRequestBody
    {
        public long ID {  get; set; }

       [Display(Name = "کد خدمات دهنده")]
       [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long StylistID { get; set; }

        [Display(Name = "کد مشتری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        [Range(1, long.MaxValue, ErrorMessage = "مقدار {0} باید بزرگتر از 0 باشد")]
        public long CustomerID { get; set; }

        [Display(Name = "تاریخ نوبت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public DateTime BookingDate { get; set; }

        [Display(Name = "زمان نوبت")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public TimeSpan BookingTime { get; set; }

        [Display(Name = "وضعیت رزرو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public string Status { get; set; }

        [Display(Name = "وضعیت لغو")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public bool IsCancelled { get; set; }

        [Display(Name = "علت لغو")]
        public string? CancelReason { get; set; }

        public string? Description { get; set; }

        public List<long> ServiceIds { get; set; } = new List<long>();



    }
}

