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
        public long BookingID { get; set; }
        public decimal AllPaymentAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal TotalServiceAmount { get; set; }
        public decimal StylistAmount { get; set; }
        public decimal PlarformAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }

        public Booking Booking { get; set; }
    }

   public class CalcPaymentDTO
    {
        public decimal AllPaymentAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal TotalServiceAmount { get; set; }
        public decimal StylistAmount { get; set; }
        public decimal PlatformAmount { get; set; }

    }
}