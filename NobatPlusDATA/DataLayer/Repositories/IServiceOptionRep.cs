using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IServiceOptionRep
    {
        Task<ListResultObject<ServiceOption>> GetAllServiceOptionsAsync(long serviceManagementId = 0, int isActive = -1, int pageIndex = 1, int pageSize = 20, string searchText = "", string sortQuery = "");
        Task<RowResultObject<ServiceOption>> GetServiceOptionByIdAsync(long serviceOptionId);
        Task<BitResultObject> AddServiceOptionAsync(ServiceOption serviceOption);
        Task<BitResultObject> EditServiceOptionAsync(ServiceOption serviceOption);
        Task<BitResultObject> RemoveServiceOptionAsync(long serviceOptionId);
        Task<BitResultObject> ExistServiceOptionAsync(long serviceOptionId);
    }
}
