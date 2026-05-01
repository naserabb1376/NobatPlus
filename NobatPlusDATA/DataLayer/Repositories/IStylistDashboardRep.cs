using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IStylistDashboardRep
    {
        Task<RowResultObject<StylistDashboardReport>> GetStylistDashboardReportAsync(long stylistId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
