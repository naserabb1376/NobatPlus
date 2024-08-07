using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class BookingService
    {
        public int BookingID { get; set; }
        public Booking Booking { get; set; }

        public int ServiceManagementID { get; set; }
        public ServiceManagement ServiceManagement { get; set; }
    }
}