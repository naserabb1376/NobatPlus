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
        public long WorkerID { get; set; }
        public long CustomerID { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
        public string CustomerName { get; set; }
    }
}