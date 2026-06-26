using NobatPlusAPI.Models.Public;

namespace NobatPlusAPI.Models.AdminAuditLog
{
    public class GetAdminAuditLogListRequestBody : GetListRequestBody
    {
        public long ActorPersonId { get; set; } = 0;
        public string ActionName { get; set; } = "";
        public string EntityName { get; set; } = "";
        public bool? Succeeded { get; set; } = null;
        public DateTime? FromDate { get; set; } = null;
        public DateTime? ToDate { get; set; } = null;
    }
}
