using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class ServiceManagement : BaseEntity
    {
        public long ServiceParentID { get; set; }
        public string ServiceName { get; set; }
        public char ServiceGender { get; set; }
      //  public TimeSpan Duration { get; set; }
     //   public long Price { get; set; }

        public ICollection<BookingService> BookingServices { get; set; }
        public ICollection<StylistService> StylistServices { get; set; }
        public ICollection<ServiceDiscount> ServiceDiscounts { get; set; }
    }
    public class ServiceManagementDTO : BaseEntity
    {
        public long ServiceParentID { get; set; }
        public string ServiceName { get; set; }
        public char ServiceGender { get; set; }
        public int StylistCount { get; set; }
    }
}