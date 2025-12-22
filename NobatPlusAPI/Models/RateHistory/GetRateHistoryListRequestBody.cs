using Domain;
using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.RateHistory
{
    public class GetRateHistoryListRequestBody : GetListRequestBody
    {
        [Display(Name = "کد مشتری")]
        public long CustomerId { get; set; } = 0;

        [Display(Name = "کد رزرو")]
        public long BookingId { get; set; } = 0;

        [Display(Name = "کد آرایشگر")]
        public long StylistId { get; set; } = 0;

        [Display(Name = "کد سوال")]
        public long RateQuestionId { get; set; } = 0;
    }
}
