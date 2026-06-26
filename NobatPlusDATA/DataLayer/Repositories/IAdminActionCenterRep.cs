using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.ViewModels;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IAdminActionCenterRep
    {
        Task<RowResultObject<AdminActionCenterVM>> GetActionCenterAsync(
            string type = "",
            string severity = "",
            int maxItemsPerType = 20);
    }
}
