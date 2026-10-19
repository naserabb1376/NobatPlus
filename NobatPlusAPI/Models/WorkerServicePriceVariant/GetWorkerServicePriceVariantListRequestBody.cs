using NobatPlusAPI.Models.Public;

namespace NobatPlusAPI.Models.WorkerServicePriceVariant
{
    public class GetWorkerServicePriceVariantListRequestBody : GetListRequestBody
    {
        public long WorkerID { get; set; } = 0;
        public long ServiceManagementID { get; set; } = 0;
        public int IsActive { get; set; } = -1;
        public bool OnlyLeafServices { get; set; } = false;
    }
}
