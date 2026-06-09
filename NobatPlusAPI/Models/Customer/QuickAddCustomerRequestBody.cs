using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.Customer
{
    public class QuickAddCustomerRequestBody
    {
        [Required]
        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        [Required]
        [RegularExpression(@"^([0-9]{11})$")]
        public string PhoneNumber { get; set; } = "";

        public int Gender { get; set; } = 1;
        public DateTime? DateOfBirth { get; set; }
        public long? CityID { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressPostalCode { get; set; }
        public string? Description { get; set; }
    }
}
