
using Domains;
using NobatPlusDATA.Domain;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusDATA.ViewModels
{
    public class StylistVM : BaseEntity
    {
        public long StylistParentID { get; set; }
        public long PersonID { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonNationalCode { get; set; }
        public string PersonPhoneNumber { get; set; }
        public string? Specialty { get; set; }
        public string StylistName { get; set; }
        public string? StylistBio { get; set; }
        public string GenderAccepted { get; set; }
        public string WorkShopInteractMode { get; set; }
        public bool IsWorkShop { get; set; }
        public string AccountStatus { get; set; }
        public string PayMethod { get; set; }
        public long WorkShopRentAmount { get; set; }
        public long WorkShopDepositAmount { get; set; }
        public int YearsOfExperience { get; set; }
        public long JobTypeID { get; set; }
        public TimeSpan RestTime { get; set; }
        public string JobTypeTitle { get; set; }
        public float AvgScoreForStylist { get; set; }
        public int TodayBookingsCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public int TotalCustomersCount { get; set; }
        public bool IsOnLeaveNow { get; set; }
        public int Gender { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string AddressStreet { get; set; }
        public string AddressPostalCode { get; set; }
        public string? AddressLocationHorizentalPoint { get; set; }
        public string? AddressLocationVerticalPoint { get; set; }
        public string? StylistImagePath { get; set; }
        public bool IsActive { get; set; }
        public List<string> ServiceNames { get; set; }
        public List<WorkTimeDTO> WorkTimes { get; set; }
        public List<SocialNetworkDTO> SocialNetworks { get; set; }


    }
}