using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Stylist : BaseEntity
    {
        public long StylistParentID { get; set; }
        public long PersonID { get; set; }
        public string? Specialty { get; set; }
        public int YearsOfExperience { get; set; }
        public long JobTypeID { get; set; }
        public JobType JobType { get; set; }
        public Person Person { get; set; }
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<StylistService> StylistServices { get; set; }
        public ICollection<DiscountAssignment> DiscountAssignments { get; set; }
        public ICollection<ServiceDiscount> ServiceDiscounts { get; set; }
        public ICollection<CustomerDiscount> CustomerDiscounts { get; set; }
    }
}