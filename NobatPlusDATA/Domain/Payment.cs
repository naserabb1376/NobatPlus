using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Payment : BaseEntity
    {
        public int BookingID { get; set; }
        public long Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }

        public Booking Booking { get; set; }
    }
}