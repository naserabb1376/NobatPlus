using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Customer : BaseEntity
    {
        public long PersonID { get; set; }
        public Person Person { get; set; }

        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<CustomerDiscount> CustomerDiscounts { get; set; }
    }
}