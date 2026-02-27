using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class PaymentDetailVM :BaseEntity
    {
        public long PaymentID { get; set; }
        public long StylistServiceID { get; set; }
        public decimal StylistServiceAmount { get; set; }
        public decimal DiscountAmount { get; set; }

        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }
        public string ServiceTitle { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }

        public long BookingID { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public bool PaymentFinished { get; set; }
        public int PaymentLevel { get; set; }


    }
}