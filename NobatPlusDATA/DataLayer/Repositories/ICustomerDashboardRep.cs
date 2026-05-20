using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface ICustomerDashboardRep
    {
        Task<RowResultObject<CustomerDashboardReport>> GetCustomerDashboardReportAsync(long personId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
