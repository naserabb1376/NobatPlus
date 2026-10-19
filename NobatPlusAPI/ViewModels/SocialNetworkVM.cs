using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class SocialNetworkVM : BaseEntity
    {
        public long WorkerID { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
        public string SocialNetworkName { get; set; }
        public string PhoneNumber { get; set; }
        public string AccountLink { get; set; }
        public string SocialNetworkIcon { get; set; }

    }
}