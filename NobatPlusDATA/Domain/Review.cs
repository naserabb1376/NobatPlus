using Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class Review : BaseEntity
    {
        public long BookingID { get; set; }
        public long CustomerID { get; set; }
        public long StylistID { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public string Status { get; set; }

        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }

        //[NotMapped]   // ⛔ در دیتابیس ساخته نمی‌شود
        public bool IsPrivate { get; set; }

        //[NotMapped]   // ⛔ در دیتابیس ساخته نمی‌شود
        public bool IsAccepted { get; set; }

        public DateTime ReviewDate { get; set; }

        public Booking Booking { get; set; }
        public Customer Customer { get; set; }
        public Stylist Stylist { get; set; }
    }
}