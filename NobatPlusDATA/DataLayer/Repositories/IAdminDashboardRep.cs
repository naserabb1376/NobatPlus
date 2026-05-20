using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IAdminDashboardRep
    {
        Task<RowResultObject<AdminDashboardReport>> GetAdminDashboardReportAsync(DateTime? fromDate = null, DateTime? toDate = null, long cityId = 0, long roleId = 0);
    }
}
