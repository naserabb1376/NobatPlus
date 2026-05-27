using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class StylistService
    {
        public long StylistID { get; set; }
        public Stylist Stylist { get; set; }

        public long ServiceManagementID { get; set; }
        public ServiceManagement ServiceManagement { get; set; }
       public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
        public bool HasDynamicPricing { get; set; }

        public ICollection<StylistServicePriceVariant> PriceVariants { get; set; }
    }

    public class ServiceOption : BaseEntity
    {
        public long ServiceManagementID { get; set; }
        public string OptionName { get; set; }
        public string OptionKey { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public ServiceManagement ServiceManagement { get; set; }
        public ICollection<ServiceOptionValue> Values { get; set; }
    }

    public class ServiceOptionValue : BaseEntity
    {
        public long ServiceOptionID { get; set; }
        public string ValueName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public ServiceOption ServiceOption { get; set; }
    }

    public class StylistServicePriceVariant : BaseEntity
    {
        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public int DepositPercent { get; set; }
        public bool IsActive { get; set; } = true;

        public StylistService StylistService { get; set; }
        public ICollection<StylistServicePriceVariantOptionValue> OptionValues { get; set; }
    }

    public class StylistServicePriceVariantOptionValue
    {
        public long StylistServicePriceVariantID { get; set; }
        public long ServiceOptionValueID { get; set; }

        public StylistServicePriceVariant StylistServicePriceVariant { get; set; }
        public ServiceOptionValue ServiceOptionValue { get; set; }
    }

    public class StylistServiceWithDiscountDto
    {
        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }
        public long BookingID { get; set; }

        public string ServiceTitle { get; set; }
        public string ServiceDescription { get; set; }

        public string StylistName { get; set; }
        public string SalonName { get; set; }

        public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
        public bool HasDynamicPricing { get; set; }
        public long? StylistServicePriceVariantID { get; set; }
        public List<long> AppliedOptionValueIDs { get; set; } = new List<long>();
        public string AppliedOptionSummary { get; set; }

        // ✅ new fields
        public int DiscountPercent { get; set; }          // درصد تخفیف اعمال‌شده
        public decimal PriceAfterDiscount { get; set; }   // قیمت بعد از تخفیف
    }
}
