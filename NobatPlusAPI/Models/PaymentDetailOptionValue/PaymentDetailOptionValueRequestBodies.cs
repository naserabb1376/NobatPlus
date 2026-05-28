using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.PaymentDetailOptionValue
{
    public class GetPaymentDetailOptionValueListRequestBody : GetListRequestBody
    {
        public long PaymentDetailID { get; set; }
        public long ServiceOptionValueID { get; set; }
    }

    public class PaymentDetailOptionValueRowRequestBody
    {
        [Range(1, long.MaxValue)]
        public long PaymentDetailID { get; set; }

        [Range(1, long.MaxValue)]
        public long ServiceOptionValueID { get; set; }
    }
}
