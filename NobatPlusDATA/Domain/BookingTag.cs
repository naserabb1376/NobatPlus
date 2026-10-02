using Domains;

namespace NobatPlusDATA.Domain
{
    public class BookingTag : BaseEntity
    {
        public long StylistID { get; set; }
        public string Title { get; set; } = "";
        public string Color { get; set; } = "";
        public int SortOrder { get; set; }

        public Stylist Stylist { get; set; }
        public ICollection<StylistServicePriceVariant> PriceVariants { get; set; } = new List<StylistServicePriceVariant>();
        public ICollection<StylistScheduleBlock> ScheduleBlocks { get; set; } = new List<StylistScheduleBlock>();
    }
}
