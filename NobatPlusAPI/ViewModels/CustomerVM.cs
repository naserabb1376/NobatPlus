using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.ViewModels
{
    public class CustomerVM : BaseEntity
    {
        public long PersonID { get; set; }
        public string PersonFullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string NaCode { get; set; }
        public int Gender { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateOfBirth { get; set; }
        public long? AddressID { get; set; }
        public long? CityID { get; set; }
        public string CityName { get; set; }
        public string AddressStreet { get; set; }
        public string AddressPostalCode { get; set; }
        public string CustomerDescription { get; set; }
        public int BookingCount { get; set; }
        public DateTime? LastBookingDate { get; set; }

    }
}
