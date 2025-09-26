using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class StylistService
    {
        public long StylistID { get; set; }
        public Stylist Stylist { get; set; }

        public long ServiceManagementID { get; set; }
        public ServiceManagement ServiceManagement { get; set; }
       public long ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }
    }
}