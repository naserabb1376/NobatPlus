using NobatPlusAPI.Models.Public;

namespace NobatPlusAPI.Models.StylistServicePriceVariant
{
    public class GetStylistServicePriceVariantListRequestBody : GetListRequestBody
    {
        public long StylistID { get; set; } = 0;
        public long ServiceManagementID { get; set; } = 0;
        public int IsActive { get; set; } = -1;
        public bool OnlyLeafServices { get; set; } = false;
    }
}
