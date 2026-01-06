using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class PaymentHistory : BaseEntity
    {
        public long BookingID { get; set; }
        public long PaymentID { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }

        public Booking Booking { get; set; }
        public Payment Payment { get; set; }
    }
}