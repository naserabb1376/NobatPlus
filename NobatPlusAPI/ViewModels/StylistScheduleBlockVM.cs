using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class StylistScheduleBlockVM : BaseEntity
    {
        public long StylistID { get; set; }
        public string StylistName { get; set; } = "";
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public long? ServiceManagementID { get; set; }
        public string ServiceName { get; set; } = "";
        public long? StylistServicePriceVariantID { get; set; }
        public long? BookingTagID { get; set; }
        public string BookingTagTitle { get; set; } = "";
        public string BookingTagColor { get; set; } = "";
        public string Title { get; set; } = "";
        public decimal ServicePrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public int DepositPercent { get; set; }
        public bool IsBookable { get; set; }
        public string Status { get; set; } = "available";
        public long? BookingID { get; set; }
        public string RowVersion { get; set; } = "";
        public List<long> OptionValueIDs { get; set; } = new();
        public string OptionSummary { get; set; } = "";
    }
}
