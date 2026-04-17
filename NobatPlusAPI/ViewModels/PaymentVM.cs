using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class PaymentVM :BaseEntity
    {
        public long BookingID { get; set; }
        public long PaymentID { get; set; }
        public long StylistID { get; set; }
        public long CustomerID { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string CustomerName { get; set; }
        public decimal AllPaymentAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal PayedAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public decimal TotalServiceAmount { get; set; }
        public decimal DiscountedServiceAmount { get; set; }
        public decimal PlatformAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal StylistAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public bool PaymentFinished { get; set; }
        public int PaymentLevel { get; set; }
        public List<PaymentItemVM> PaymentItems { get; set; }

    }
}