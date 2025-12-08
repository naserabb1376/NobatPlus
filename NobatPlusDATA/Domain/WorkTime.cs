using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class WorkTime : BaseEntity
    {
        public long StylistID { get; set; }
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public string DayOfWeek { get; set; }

        public Stylist Stylist { get; set; }
    }

    public class WorkTimeDTO
    {
        public TimeSpan WorkStartTime { get; set; }
        public TimeSpan WorkEndTime { get; set; }
        public string DayOfWeek { get; set; }
    }
}