using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class PaymentDetail : BaseEntity
    {
        public long PaymentID { get; set; }
        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }
        public decimal StylistServiceAmount { get; set; }
        public int DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }


        [ForeignKey("PaymentID")]
        public Payment Payment { get; set; }

        [ForeignKey("StylistID")]
        public Stylist Stylist { get; set; }

        [ForeignKey("ServiceManagementID")]
        public ServiceManagement ServiceManagement { get; set; }

        //public StylistService StylistService { get; set; }

    }
}