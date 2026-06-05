using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class ServiceOptionVM : BaseEntity
    {
        public long ServiceManagementID { get; set; }
        public string ServiceName { get; set; }
        public string OptionName { get; set; }
        public string OptionKey { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public List<ServiceOptionValueVM> Values { get; set; }
    }

    public class ServiceOptionValueVM : BaseEntity
    {
        public long ServiceOptionID { get; set; }
        public string OptionName { get; set; }
        public string ValueName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class StylistServicePriceVariantVM : BaseEntity
    {
        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public int DepositPercent { get; set; }
        public bool IsActive { get; set; }
        public string OptionValueCombinationKey { get; set; }
        public List<long> OptionValueIDs { get; set; }
        public string OptionSummary { get; set; }
    }
}
