using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer.Services
{
    public class AdminAuditLogRep : IAdminAuditLogRep
    {
        private readonly NobatPlusContext _context;

        public AdminAuditLogRep(NobatPlusContext context)
        {
            _context = context;
        }

        public async Task<BitResultObject> AddAdminAuditLogAsync(AdminAuditLog auditLog)
        {
            var result = new BitResultObject();
            try
            {
                await _context.AdminAuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
                result.ID = auditLog.ID;
                _context.Entry(auditLog).State = EntityState.Detached;
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }

        public async Task<ListResultObject<AdminAuditLog>> GetAllAdminAuditLogsAsync(
            int pageIndex = 1,
            int pageSize = 20,
            string searchText = "",
            string sortQuery = "",
            long actorPersonId = 0,
            string actionName = "",
            string entityName = "",
            bool? succeeded = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var results = new ListResultObject<AdminAuditLog>();
            try
            {
                var query = _context.AdminAuditLogs
                    .AsNoTracking()
                    .Include(x => x.ActorPerson)
                    .AsQueryable();

                if (actorPersonId > 0)
                {
                    query = query.Where(x => x.ActorPersonID == actorPersonId);
                }

                if (!string.IsNullOrWhiteSpace(actionName))
                {
                    query = query.Where(x => x.ActionName.Contains(actionName));
                }

                if (!string.IsNullOrWhiteSpace(entityName))
                {
                    query = query.Where(x => x.EntityName.Contains(entityName) || x.ControllerName.Contains(entityName));
                }

                if (succeeded.HasValue)
                {
                    query = query.Where(x => x.Succeeded == succeeded.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.OccurredAt >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(x => x.OccurredAt <= toDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.ActorFullName.Contains(searchText) ||
                        x.ActionName.Contains(searchText) ||
                        x.ControllerName.Contains(searchText) ||
                        x.EntityName.Contains(searchText) ||
                        (x.TargetId != null && x.TargetId.Contains(searchText)) ||
                        (x.RequestPath != null && x.RequestPath.Contains(searchText)) ||
                        (x.RequestSummary != null && x.RequestSummary.Contains(searchText)) ||
                        (x.ErrorMessage != null && x.ErrorMessage.Contains(searchText)) ||
                        (x.IpAddress != null && x.IpAddress.Contains(searchText)) ||
                        (x.Description != null && x.Description.Contains(searchText)));
                }

                results.TotalCount = await query.CountAsync();
                results.PageCount = DbTools.GetPageCount(results.TotalCount, pageSize);
                results.Results = await query
                    .OrderByDescending(x => x.OccurredAt)
                    .SortBy(sortQuery)
                    .ToPaging(pageIndex, pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                results.Status = false;
                results.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return results;
        }

        public async Task<RowResultObject<AdminAuditLog>> GetAdminAuditLogByIdAsync(long id)
        {
            var result = new RowResultObject<AdminAuditLog>();
            try
            {
                result.Result = await _context.AdminAuditLogs
                    .AsNoTracking()
                    .Include(x => x.ActorPerson)
                    .SingleOrDefaultAsync(x => x.ID == id);
            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message} - {ex.InnerException?.Message}";
            }
            return result;
        }
    }
}
