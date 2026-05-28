using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class BookingService
    {
        public long BookingID { get; set; }
        public Booking Booking { get; set; }

        public long ServiceManagementID { get; set; }
        public ServiceManagement ServiceManagement { get; set; }

        public ICollection<BookingServiceOptionValue> OptionValues { get; set; }
    }

    public class BookingServiceOptionValue
    {
        public long BookingID { get; set; }
        public long ServiceManagementID { get; set; }
        public long ServiceOptionValueID { get; set; }

        public BookingService BookingService { get; set; }
        public ServiceOptionValue ServiceOptionValue { get; set; }
    }
}
