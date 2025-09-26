using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class BookingVM : BaseEntity
    {
        public long StylistID { get; set; }
        public long CustomerID { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public TimeSpan TotalTimeDuration { get; set; }
        public string Status { get; set; }
        public bool IsCancelled { get; set; }
        public string CancelReason { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string CustomerName { get; set; }

    }
}