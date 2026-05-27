using NobatPlusAPI.Models.Public;

namespace NobatPlusAPI.Models.ServiceOptionValue
{
    public class GetServiceOptionValueListRequestBody : GetListRequestBody
    {
        public long ServiceOptionID { get; set; } = 0;
        public int IsActive { get; set; } = -1;
    }
}
