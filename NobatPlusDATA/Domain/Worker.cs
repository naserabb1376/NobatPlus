using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Worker : BaseEntity
    {
        public long WorkerParentID { get; set; }
        public long PersonID { get; set; }
        public string? Specialty { get; set; }
        public string WorkerName { get; set; }
        public string? WorkerBio { get; set; }
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
        public int SlotIntervalMinutes { get; set; }
        public string BookingCreationMode { get; set; }
        public string SlotDisplayMode { get; set; }
        public JobType JobType { get; set; }
        public Person Person { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<RateHistory> RateHistories { get; set; }
        public ICollection<WorkTime> WorkTimes { get; set; }
        public ICollection<WorkerPacific> WorkerPacifics { get; set; }
        public ICollection<SocialNetwork> SocialNetworks { get; set; }
        public ICollection<WorkerService> WorkerServices { get; set; }
        public ICollection<DiscountAssignment> DiscountAssignments { get; set; }
        public ICollection<ServiceDiscount> ServiceDiscounts { get; set; }
        public ICollection<CustomerDiscount> CustomerDiscounts { get; set; }
        public ICollection<PaymentDetail> PaymentDetails { get; set; }
        public FinancialAccount FinancialAccount { get; set; }
        public ICollection<BookingTag> BookingTags { get; set; }
        public ICollection<WorkerScheduleBlock> ScheduleBlocks { get; set; }
    }

    public class WorkerDTO : BaseEntity
    {
        public long WorkerParentID { get; set; }
        public long PersonID { get; set; }
        public string? Specialty { get; set; }
        public string WorkerName { get; set; }
        public string? WorkerBio { get; set; }
        public string GenderAccepted { get; set; }
        public string WorkShopInteractMode { get; set; }
        public bool IsWorkShop { get; set; }
        public string AccountStatus { get; set; }
        public string PayMethod { get; set; }
        public long WorkShopRentAmount { get; set; }
        public long WorkShopDepositAmount { get; set; }
        public int YearsOfExperience { get; set; }
        public float AvgScoreForWorker { get; set; }
        public long JobTypeID { get; set; }
        public int TodayBookingsCount { get; set; }
        public int WorkshopWorkerCount { get; set; }
        public int SuccededBokingCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public int TotalCustomersCount { get; set; }
        public bool IsOnLeaveNow { get; set; }
        public string? WorkerImagePath { get; set; }
        public double RecommendPercent { get; set; }
        public TimeSpan RestTime { get; set; }
        public int SlotIntervalMinutes { get; set; }
        public string BookingCreationMode { get; set; }
        public string SlotDisplayMode { get; set; }
        public JobType JobType { get; set; }
        public Person Person { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<RateHistory> RateHistories { get; set; }
        public ICollection<WorkTimeDTO> WorkTimes { get; set; }
        public ICollection<SocialNetworkDTO> SocialNetworks { get; set; }
        public ICollection<WorkerService> WorkerServices { get; set; }
        public ICollection<DiscountAssignment> DiscountAssignments { get; set; }
        public ICollection<ServiceDiscount> ServiceDiscounts { get; set; }
        public ICollection<CustomerDiscount> CustomerDiscounts { get; set; }
    }

    public class WorkerProfileDTO
    {
        public long ID { get; set; }
        public long PersonID { get; set; }
        public long WorkerParentID { get; set; }
        public long JobTypeId { get; set; }
        public string Name { get; set; } = "";
        public string ProfileType { get; set; } = "";
        public string JobTitle { get; set; }
        public bool IsActive { get; set; }
        public string AccountStatus { get; set; } = "";
    }
}
