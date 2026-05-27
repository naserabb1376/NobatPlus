using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IServiceOptionValueRep
    {
        Task<ListResultObject<ServiceOptionValue>> GetAllServiceOptionValuesAsync(long serviceOptionId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<ServiceOptionValue>> GetServiceOptionValueByIdAsync(long serviceOptionValueId);
        Task<BitResultObject> AddServiceOptionValueAsync(ServiceOptionValue serviceOptionValue);
        Task<BitResultObject> EditServiceOptionValueAsync(ServiceOptionValue serviceOptionValue);
        Task<BitResultObject> RemoveServiceOptionValueAsync(long serviceOptionValueId);
        Task<BitResultObject> ExistServiceOptionValueAsync(long serviceOptionValueId);
    }
}
