using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class StylistServiceVM
    {
        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }

        public string ServiceTitle { get; set; }
        public string ServiceDescription { get; set; }

        public string StylistName { get; set; }
        public string SalonName { get; set; }

        public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }

        public int DiscountPercent { get; set; }          // درصد تخفیف اعمال‌شده
        public decimal PriceAfterDiscount { get; set; }   // قیمت بعد از تخفیف

    }
}