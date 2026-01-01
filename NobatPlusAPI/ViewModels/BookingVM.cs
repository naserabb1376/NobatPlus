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
        public bool IsCancelled { get; set; }
        public string CancelReason { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }

    }
}