using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class WorkerServiceVM
    {
        public long WorkerID { get; set; }
        public long ServiceManagementID { get; set; }

        public string ServiceTitle { get; set; }
        public string ServiceDescription { get; set; }

        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }

        public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
        public bool HasDynamicPricing { get; set; }
        public string? Description { get; set; }
        public long? WorkerServicePriceVariantID { get; set; }
        public List<long> AppliedOptionValueIDs { get; set; } = new();
        public string AppliedOptionSummary { get; set; } = "";

        public int DiscountPercent { get; set; }          // درصد تخفیف اعمال‌شده
        public decimal PriceAfterDiscount { get; set; }   // قیمت بعد از تخفیف
        public List<WorkerServicePriceVariantPriceVM> PriceVariants { get; set; } = new();

    }

    public class WorkerServicePriceVariantPriceVM
    {
        public long WorkerServicePriceVariantID { get; set; }
        public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
        public List<long> AppliedOptionValueIDs { get; set; } = new();
        public string AppliedOptionSummary { get; set; } = "";
        public int DiscountPercent { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public long? BookingTagID { get; set; }
        public string BookingTagTitle { get; set; } = "";
        public string BookingTagColor { get; set; } = "";
    }
}
