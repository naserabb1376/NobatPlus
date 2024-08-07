using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Booking : BaseEntity
    {
        public int StylistID { get; set; }
        public int CustomerID { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public string Status { get; set; }
        public bool IsCancelled { get; set; }
        public string CancelReason { get; set; }

        public Stylist Stylist { get; set; }
        public Customer Customer { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<BookingService> BookingServices { get; set; }
    }
}