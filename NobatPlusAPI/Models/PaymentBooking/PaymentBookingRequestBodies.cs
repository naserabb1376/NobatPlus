using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PaymentBooking
{
    public class GetPaymentBookingListRequestBody : GetListRequestBody
    {
        public long PaymentID { get; set; }
        public long BookingID { get; set; }
    }

    public class PaymentBookingRowRequestBody
    {
        [Range(1, long.MaxValue)]
        public long PaymentID { get; set; }

        [Range(1, long.MaxValue)]
        public long BookingID { get; set; }
    }
}
