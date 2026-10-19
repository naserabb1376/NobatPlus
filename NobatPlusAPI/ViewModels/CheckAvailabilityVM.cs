using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class CheckAvailabilityVM :BaseEntity
    {
        public long WorkerID { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
    }
}