using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.ViewModels;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IAdminMonitoringRep
    {
        Task<RowResultObject<AdminMonitoringReportVM>> GetMonitoringReportAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}
