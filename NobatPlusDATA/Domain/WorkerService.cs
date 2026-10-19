using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class WorkerService
    {
        public long WorkerID { get; set; }
        public Worker Worker { get; set; }

        public long ServiceManagementID { get; set; }
        public ServiceManagement ServiceManagement { get; set; }
       public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
        public bool HasDynamicPricing { get; set; }
        public string? Description { get; set; }

        public ICollection<WorkerServicePriceVariant> PriceVariants { get; set; }
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

    public class WorkerServicePriceVariant : BaseEntity
    {
        public long WorkerID { get; set; }
        public long ServiceManagementID { get; set; }
        public string OptionValueCombinationKey { get; set; } = "";
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public int DepositPercent { get; set; }
        public bool IsActive { get; set; } = true;
        public long? BookingTagID { get; set; }

        public WorkerService WorkerService { get; set; }
        public BookingTag? BookingTag { get; set; }
        public ICollection<WorkerServicePriceVariantOptionValue> OptionValues { get; set; }

        public static string BuildOptionValueCombinationKey(IEnumerable<long>? optionValueIds)
        {
            return string.Join("|", (optionValueIds ?? Enumerable.Empty<long>())
                .Where(x => x > 0)
                .Distinct()
                .OrderBy(x => x));
        }
    }

    public class WorkerServicePriceVariantOptionValue
    {
        public long WorkerServicePriceVariantID { get; set; }
        public long ServiceOptionValueID { get; set; }

        public WorkerServicePriceVariant WorkerServicePriceVariant { get; set; }
        public ServiceOptionValue ServiceOptionValue { get; set; }
    }

    public class WorkerServiceWithDiscountDto
    {
        public long WorkerID { get; set; }
        public long ServiceManagementID { get; set; }
        public long BookingID { get; set; }

        public string ServiceTitle { get; set; }
        public string ServiceDescription { get; set; }

        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }

        public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
        public bool HasDynamicPricing { get; set; }
        public long? WorkerServicePriceVariantID { get; set; }
        public List<long> AppliedOptionValueIDs { get; set; } = new List<long>();
        public string AppliedOptionSummary { get; set; }

        // ✅ new fields
        public int DiscountPercent { get; set; }          // درصد تخفیف اعمال‌شده
        public decimal PriceAfterDiscount { get; set; }   // قیمت بعد از تخفیف
        public List<WorkerServicePriceVariantPriceDto> PriceVariants { get; set; } = new List<WorkerServicePriceVariantPriceDto>();
    }

    public class WorkerServicePriceVariantPriceDto
    {
        public long WorkerServicePriceVariantID { get; set; }
        public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
        public List<long> AppliedOptionValueIDs { get; set; } = new List<long>();
        public string AppliedOptionSummary { get; set; } = "";
        public int DiscountPercent { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public long? BookingTagID { get; set; }
        public string BookingTagTitle { get; set; } = "";
        public string BookingTagColor { get; set; } = "";
    }
}
