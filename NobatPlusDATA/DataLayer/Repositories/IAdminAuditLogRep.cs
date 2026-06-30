using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;

namespace NobatPlusDATA.DataLayer.Repositories
{
    public interface IAdminAuditLogRep
    {
        Task<ListResultObject<AdminAuditLog>> GetAllAdminAuditLogsAsync(
            int pageIndex = 1,
            int pageSize = 20,
            string searchText = "",
            string sortQuery = "",
            long actorPersonId = 0,
            string actionName = "",
            string entityName = "",
            string targetId = "",
            bool? succeeded = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<RowResultObject<AdminAuditLog>> GetAdminAuditLogByIdAsync(long id);
        Task<BitResultObject> AddAdminAuditLogAsync(AdminAuditLog auditLog);
    }
}
