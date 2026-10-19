using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class WorkTimeVM : BaseEntity
    {
        public long WorkerId { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public TimeSpan WorkerRestTime { get; set; }
        public string DayOfWeek { get; set; }

    }
}