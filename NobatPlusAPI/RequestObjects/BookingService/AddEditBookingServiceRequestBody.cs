using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects
{
    public class AddEditBookingServiceRequestBody
    {
        public long ID {  get; set; }

       [Display(Name = "کد خدمات دهنده")]
       [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
        public long StylistID { get; set; }

        [Display(Name = "کد مشتری")]
        [Required(ErrorMessage = "لطفا {0} را وارد کنید")]
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
        public string? CancellReason { get; set; }

        public string? Description { get; set; }


    }
}

