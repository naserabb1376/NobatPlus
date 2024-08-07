using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class DiscountAssignment : BaseEntity
    {
        public int DiscountId { get; set; }
        public int? AdminId { get; set; }
        public int? StylistId { get; set; }

        public Discount Discount { get; set; }
        public Admin Admin { get; set; }
        public Stylist Stylist { get; set; }
    }
}