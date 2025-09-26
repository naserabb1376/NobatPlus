using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class WorkTimeVM : BaseEntity
    {
        public long StylistId { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public TimeSpan StylistRestTime { get; set; }
        public string DayOfWeek { get; set; }

    }
}