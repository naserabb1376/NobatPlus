using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Admin : BaseEntity
    {
        public int PersonID { get; set; }
        public string Role { get; set; }

        public Person Person { get; set; }
        public ICollection<DiscountAssignment> DiscountAssignments { get; set; }
        public ICollection<ServiceDiscount> ServiceDiscounts { get; set; }
    }
}