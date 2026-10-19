using NobatPlusAPI.Models.Public;
using System.ComponentModel.DataAnnotations;

namespace NobatPlusAPI.Models.WorkerServicePriceVariantOptionValue
{
    public class GetWorkerServicePriceVariantOptionValueListRequestBody : GetListRequestBody
    {
        public long WorkerServicePriceVariantID { get; set; }
        public long ServiceOptionValueID { get; set; }
    }

    public class WorkerServicePriceVariantOptionValueRowRequestBody
    {
        [Range(1, long.MaxValue)]
        public long WorkerServicePriceVariantID { get; set; }

        [Range(1, long.MaxValue)]
        public long ServiceOptionValueID { get; set; }
    }
}
