using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Stylist : BaseEntity
    {
        public long StylistParentID { get; set; }
        public long PersonID { get; set; }
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
        public JobType JobType { get; set; }
        public Person Person { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<RateHistory> RateHistories { get; set; }
        public ICollection<WorkTime> WorkTimes { get; set; }
        public ICollection<StylistPacific> StylistPacifics { get; set; }
        public ICollection<SocialNetwork> SocialNetworks { get; set; }
        public ICollection<StylistService> StylistServices { get; set; }
        public ICollection<DiscountAssignment> DiscountAssignments { get; set; }
        public ICollection<ServiceDiscount> ServiceDiscounts { get; set; }
        public ICollection<CustomerDiscount> CustomerDiscounts { get; set; }
    }

    public class StylistDTO : BaseEntity
    {
        public long StylistParentID { get; set; }
        public long PersonID { get; set; }
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
        public float AvgScoreForStylist { get; set; }
        public long JobTypeID { get; set; }
        public int TodayBookingsCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public int TotalCustomersCount { get; set; }
        public bool IsOnLeaveNow { get; set; }
        public string? StylistImagePath { get; set; }
        public double RecommendPercent { get; set; }
        public TimeSpan RestTime { get; set; }
        public JobType JobType { get; set; }
        public Person Person { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<RateHistory> RateHistories { get; set; }
        public ICollection<WorkTimeDTO> WorkTimes { get; set; }
        public ICollection<SocialNetworkDTO> SocialNetworks { get; set; }
        public ICollection<StylistService> StylistServices { get; set; }
        public ICollection<DiscountAssignment> DiscountAssignments { get; set; }
        public ICollection<ServiceDiscount> ServiceDiscounts { get; set; }
        public ICollection<CustomerDiscount> CustomerDiscounts { get; set; }
    }
}