using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IWorkerDashboardRep
    {
        Task<RowResultObject<WorkerDashboardReport>> GetWorkerDashboardReportAsync(long workerId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<RowResultObject<WorkshopDashboardReport>> GetWorkshopDashboardReportAsync(long workshopWorkerId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
