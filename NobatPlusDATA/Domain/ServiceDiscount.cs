using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class ServiceDiscount : BaseEntity
    {
        public long DiscountId { get; set; }
        public long ServiceManagementId { get; set; }
        public long? AdminId { get; set; }
        public long? WorkerId { get; set; }

        public Discount Discount { get; set; }
        public ServiceManagement ServiceManagement { get; set; }
        public Admin Admin { get; set; }
        public Worker Worker { get; set; }
    }
}