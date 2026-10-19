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
        public long BookingID { get; set; }
        public long WorkerID { get; set; }
        public long ServiceManagementID { get; set; }
        public long? WorkerServicePriceVariantID { get; set; }
        public decimal WorkerServiceAmount { get; set; }
        public int DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public string AppliedOptionSummary { get; set; }


        [ForeignKey("PaymentID")]
        public Payment Payment { get; set; }

        [ForeignKey("BookingID")]
        public Booking Booking { get; set; }

        [ForeignKey("WorkerID")]
        public Worker Worker { get; set; }

        [ForeignKey("ServiceManagementID")]
        public ServiceManagement ServiceManagement { get; set; }

        public WorkerService WorkerService { get; set; }
        public WorkerServicePriceVariant WorkerServicePriceVariant { get; set; }
        public ICollection<PaymentDetailOptionValue> OptionValues { get; set; }

    }

    public class PaymentDetailOptionValue
    {
        public long PaymentDetailID { get; set; }
        public long ServiceOptionValueID { get; set; }

        public PaymentDetail PaymentDetail { get; set; }
        public ServiceOptionValue ServiceOptionValue { get; set; }
    }
}
