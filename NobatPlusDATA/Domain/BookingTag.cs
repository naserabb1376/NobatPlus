using Domains;

namespace NobatPlusDATA.Domain
{
    public class BookingTag : BaseEntity
    {
        public long WorkerID { get; set; }
        public string Title { get; set; } = "";
        public string Color { get; set; } = "";
        public int SortOrder { get; set; }

        public Worker Worker { get; set; }
        public ICollection<WorkerServicePriceVariant> PriceVariants { get; set; } = new List<WorkerServicePriceVariant>();
        public ICollection<WorkerScheduleBlock> ScheduleBlocks { get; set; } = new List<WorkerScheduleBlock>();
    }
}
