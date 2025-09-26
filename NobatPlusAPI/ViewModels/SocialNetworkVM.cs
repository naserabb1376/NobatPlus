using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class SocialNetworkVM : BaseEntity
    {
        public long StylistID { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string SocialNetworkName { get; set; }
        public string PhoneNumber { get; set; }
        public string AccountLink { get; set; }
        public string SocialNetworkIcon { get; set; }

    }
}