using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class PaymentDetailVM :BaseEntity
    {
        public long PaymentID { get; set; }
        public long BookingID { get; set; }
        public long WorkerServiceID { get; set; }
        public decimal WorkerServiceAmount { get; set; }
        public int DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public long? WorkerServicePriceVariantID { get; set; }
        public string AppliedOptionSummary { get; set; }
        public List<long> AppliedOptionValueIDs { get; set; }

        public long WorkerID { get; set; }
        public long ServiceManagementID { get; set; }
        public string ServiceTitle { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }

        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
        public bool PaymentFinished { get; set; }
        public int PaymentLevel { get; set; }


    }

    public class PaymentItemVM
    {
        public decimal WorkerServiceAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public int DiscountPercent { get; set; }
        public long? WorkerServicePriceVariantID { get; set; }
        public string AppliedOptionSummary { get; set; }
        public List<long> AppliedOptionValueIDs { get; set; }

        public long BookingID { get; set; }
        public long WorkerID { get; set; }
        public long ServiceManagementID { get; set; }
        public string ServiceTitle { get; set; }
        public string WorkerName { get; set; }
        public string? WorkshopName { get; set; }


    }
}
