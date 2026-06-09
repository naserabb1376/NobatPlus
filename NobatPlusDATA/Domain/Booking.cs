using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Booking : BaseEntity
    {
        public long StylistID { get; set; }
        public long CustomerID { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public bool IsCancelled { get; set; }
        public string CancelReason { get; set; }

        public Stylist Stylist { get; set; }
        public Customer Customer { get; set; }
        public ICollection<PaymentBooking> PaymentBookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<RateHistory> RateHistories { get; set; }
        public ICollection<BookingService> BookingServices { get; set; }
    }

    public class BookingDTO : BaseEntity
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

        public Stylist Stylist { get; set; }
        public Customer Customer { get; set; }
        public List<BookingServiceSelectionDTO>? Services { get; set; }
    }

    public class BookingServiceSelectionDTO
    {
        public long ServiceID { get; set; }
        public string? ServiceName { get; set; }
        public List<long>? OptionValueIDs { get; set; }
        public List<BookingServiceOptionValueDTO>? OptionValues { get; set; }
    }

    public class BookingServiceOptionValueDTO
    {
        public long ServiceOptionValueID { get; set; }
        public long ServiceOptionID { get; set; }
        public string? OptionName { get; set; }
        public string? ValueName { get; set; }
    }
}
