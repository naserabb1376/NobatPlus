using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class CheckAvailabilityVM :BaseEntity
    {
        public long StylistID { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
    }
}