using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class StylistService : basee
    {
        public long StylistID { get; set; }
        public Stylist Stylist { get; set; }

        public long ServiceManagementID { get; set; }
        public ServiceManagement ServiceManagement { get; set; }
       public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
    }

    public class StylistServiceWithDiscountDto
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

        // ✅ new fields
        public int DiscountPercent { get; set; }          // درصد تخفیف اعمال‌شده
        public decimal PriceAfterDiscount { get; set; }   // قیمت بعد از تخفیف
    }
}