using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Review : BaseEntity
    {
        public int BookingID { get; set; }
        public int CustomerID { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }

        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }

        public DateTime ReviewDate { get; set; }

        public Booking Booking { get; set; }
        public Customer Customer { get; set; }
    }
}