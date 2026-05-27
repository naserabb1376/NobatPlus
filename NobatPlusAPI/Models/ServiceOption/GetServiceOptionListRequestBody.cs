using NobatPlusAPI.Models.Public;

namespace NobatPlusAPI.Models.ServiceOption
{
    public class GetServiceOptionListRequestBody : GetListRequestBody
    {
        public long ServiceManagementID { get; set; } = 0;
        public int IsActive { get; set; } = -1;
    }
}
