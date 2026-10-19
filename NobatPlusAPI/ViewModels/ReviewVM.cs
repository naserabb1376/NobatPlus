using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class ReviewVM :BaseEntity
    {
        public long? BookingID { get; set; }
        public long WorkerID { get; set; }
        public long CustomerID { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
        public DateTime ReviewDate { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsAccepted { get; set; }
        public float AverageRateScore { get; set; }

    }
}