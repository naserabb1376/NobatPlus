using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class SocialNetwork : BaseEntity
    {
        public long StylistID { get; set; }
        public string SocialNetworkName { get; set; }
        public string PhoneNumber { get; set; }
        public string AccountLink { get; set; }
        public string SocialNetworkIcon { get; set; }

        public Stylist Stylist { get; set; }
    }
}