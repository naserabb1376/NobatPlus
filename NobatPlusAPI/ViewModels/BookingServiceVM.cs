using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class BookingServiceVM
    {
        public long BookingID { get; set; }
        public long ServiceManagementID { get; set; }
        public string ServiceTitle { get; set; }
        public long StylistID { get; set; }
        public long CustomerID { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string CustomerName { get; set; }
    }
}