using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.BookingServiceOptionValue
{
    public class GetBookingServiceOptionValueListRequestBody : GetListRequestBody
    {
        public long BookingID { get; set; }
        public long ServiceManagementID { get; set; }
        public long ServiceOptionValueID { get; set; }
    }

    public class BookingServiceOptionValueRowRequestBody
    {
        [Range(1, long.MaxValue)]
        public long BookingID { get; set; }

        [Range(1, long.MaxValue)]
        public long ServiceManagementID { get; set; }

        [Range(1, long.MaxValue)]
        public long ServiceOptionValueID { get; set; }
    }
}
