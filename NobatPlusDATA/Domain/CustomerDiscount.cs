using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class CustomerDiscount : BaseEntity
    {
        public int DiscountId { get; set; }
        public int CustomerId { get; set; }
        public int StylistId { get; set; }

        public Discount Discount { get; set; }
        public Customer Customer { get; set; }
        public Stylist Stylist { get; set; }
    }
}