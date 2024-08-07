using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class StylistService
    {
        public int StylistID { get; set; }
        public Stylist Stylist { get; set; }

        public int ServiceManagementID { get; set; }
        public ServiceManagement ServiceManagement { get; set; }
    }
}