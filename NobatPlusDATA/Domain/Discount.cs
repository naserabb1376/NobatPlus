using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Discount : BaseEntity
    {
        public string DiscountCode { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<DiscountAssignment> DiscountAssignments { get; set; }
        public ICollection<ServiceDiscount> ServiceDiscounts { get; set; }
        public ICollection<CustomerDiscount> CustomerDiscounts { get; set; }
    }
}