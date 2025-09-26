using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.ViewModels
{
    public class CustomerDiscountVM : BaseEntity
    {
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string CustomerName { get; set; }
        public string DiscountCode { get; set; }
        public int DiscountAmount { get; set; }

    }
}