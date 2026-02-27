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
        public long DiscountID { get; set; }
        public decimal AllPaymentAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal PayedAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public decimal TotalServiceAmount { get; set; }
        public decimal DiscountedServiceAmount { get; set; }
        public decimal StylistAmount { get; set; }
        public decimal PlarformAmount { get; set; }
        public decimal VatAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public bool PaymentFinished { get; set; }
        public int PaymentLevel { get; set; }

        public Booking Booking { get; set; }
        public Discount? Discount { get; set; }
        public ICollection<PaymentDetail> PaymentDetails { get; set; }
    }

   public class CalcPaymentDTO
    {
        public decimal AllPaymentAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal PayedAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public decimal TotalServiceAmount { get; set; }
        public decimal DiscountedServiceAmount { get; set; }
        public decimal StylistAmount { get; set; }
        public decimal PlatformAmount { get; set; }
        public decimal VatAmount { get; set; }

    }
}