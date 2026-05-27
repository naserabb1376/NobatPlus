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
        public long StylistID { get; set; }
        public long ServiceManagementID { get; set; }
        public long? StylistServicePriceVariantID { get; set; }
        public decimal StylistServiceAmount { get; set; }
        public int DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public string AppliedOptionSummary { get; set; }


        [ForeignKey("PaymentID")]
        public Payment Payment { get; set; }

        [ForeignKey("BookingID")]
        public Booking Booking { get; set; }

        [ForeignKey("StylistID")]
        public Stylist Stylist { get; set; }

        [ForeignKey("ServiceManagementID")]
        public ServiceManagement ServiceManagement { get; set; }

        public StylistService StylistService { get; set; }
        public StylistServicePriceVariant StylistServicePriceVariant { get; set; }
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
