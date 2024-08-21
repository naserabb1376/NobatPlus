using Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.RequestObjects.Authenticate
{
    public class SignupRequestBody
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsStylist { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string State { get; set; }
        public long CityID { get; set; }
        public string AddressStreet { get; set; }
        public string AddressPostalCode { get; set; }
        public string? AddressLocationHorizentalPoint { get; set; }
        public string? AddressLocationVerticalPoint { get; set; }
        public string NaCode { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Specialty { get; set; }
        public int YearsOfExperience { get; set; }
        public long StylistParentID { get; set; }
        public long JobTypeID { get; set; }
        public string? PersonDescription { get; set; }
        public string? CustomerDescription { get; set; }
        public string? RegisterDescription { get; set; }
        public string? LoginDescription { get; set; }
        public string? StylistDescription { get; set; }
        public string? AddressDescription { get; set; }
    }
}