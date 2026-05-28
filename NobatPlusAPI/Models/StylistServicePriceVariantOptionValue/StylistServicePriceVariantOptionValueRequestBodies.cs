using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.StylistServicePriceVariantOptionValue
{
    public class GetStylistServicePriceVariantOptionValueListRequestBody : GetListRequestBody
    {
        public long StylistServicePriceVariantID { get; set; }
        public long ServiceOptionValueID { get; set; }
    }

    public class StylistServicePriceVariantOptionValueRowRequestBody
    {
        [Range(1, long.MaxValue)]
        public long StylistServicePriceVariantID { get; set; }

        [Range(1, long.MaxValue)]
        public long ServiceOptionValueID { get; set; }
    }
}
