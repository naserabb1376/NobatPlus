using Domain;
using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class BookingVM : BaseEntity
    {
        public long StylistID { get; set; }
        public long CustomerID { get; set; }
        public DateTime BookingStartDate { get; set; }
        public DateTime BookingEndDate { get; set; }
        public string Status { get; set; }
        public int TotalDurationMinutes { get; set; }
        public int TotalBlockMinutes { get; set; }
        public List<long> ServiceIDs { get; set; } = new();
        public bool IsCancelled { get; set; }
        public string CancelReason { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public List<BookingSelectedServiceVM>? Services { get; set; }
        public Address? StylistAddress { get; set; }
        public long? ScheduleBlockID { get; set; }

    }

    public class BookingSelectedServiceVM
    {
        public long ServiceID { get; set; }
        public string? ServiceName { get; set; }
        public List<long>? OptionValueIDs { get; set; }
        public List<BookingSelectedServiceOptionValueVM>? OptionValues { get; set; }
        public long? StylistServicePriceVariantID { get; set; }
        public decimal? UnitPriceSnapshot { get; set; }
        public int? DiscountPercentSnapshot { get; set; }
        public decimal? PriceAfterDiscountSnapshot { get; set; }
        public int? DepositPercentSnapshot { get; set; }
        public int? DurationMinutesSnapshot { get; set; }
    }

    public class BookingSelectedServiceOptionValueVM
    {
        public long ServiceOptionValueID { get; set; }
        public long ServiceOptionID { get; set; }
        public string? OptionName { get; set; }
        public string? ValueName { get; set; }
    }

    public class PublicBookingSlotVM
    {
        public long StylistID { get; set; }
        public DateTime BookingStartDate { get; set; }
        public DateTime BookingEndDate { get; set; }
        public string Status { get; set; } = "";
        public int TotalDurationMinutes { get; set; }
        public int TotalBlockMinutes { get; set; }
        public List<long> ServiceIDs { get; set; } = new();
        public bool IsCancelled { get; set; }
        public long? ScheduleBlockID { get; set; }
        public string BookingCreationMode { get; set; } = "";
        public string Title { get; set; } = "";
        public decimal ServicePrice { get; set; }
        public int DiscountPercent { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public int DepositPercent { get; set; }
        public long? StylistServicePriceVariantID { get; set; }
        public long? BookingTagID { get; set; }
        public string BookingTagTitle { get; set; } = "";
        public string BookingTagColor { get; set; } = "";
        public List<long> OptionValueIDs { get; set; } = new();
        public string OptionSummary { get; set; } = "";
    }
}
