using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.ViewModels
{
    public class CustomerVM : BaseEntity
    {
        public long PersonID { get; set; }
        public string PersonFullName { get; set; }
        public string PhoneNumber { get; set; }
        public int BookingCount { get; set; }
        public DateTime? LastBookingDate { get; set; }

    }
}