
using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class StylistVM : BaseEntity
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
      
    }
}