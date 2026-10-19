using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class PaymentVM :BaseEntity
    {
        public long WorkerID { get; set; }
        public long CustomerID { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
        public string CustomerName { get; set; }
        public decimal AllPaymentAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal PayedAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public decimal TotalServiceAmount { get; set; }
        public decimal DiscountedServiceAmount { get; set; }
        public decimal PlatformAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal WorkerAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public bool PaymentFinished { get; set; }
        public int PaymentLevel { get; set; }
        public List<PaymentBookingItemVM> Bookings { get; set; }

    }

    public class PaymentBookingItemVM
    {
        public long BookingID { get; set; }
        public DateTime BookingDate { get; set; }
        public long WorkerID { get; set; }
        public long CustomerID { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
        public List<PaymentItemVM> PaymentItems { get; set; }
    }
}
