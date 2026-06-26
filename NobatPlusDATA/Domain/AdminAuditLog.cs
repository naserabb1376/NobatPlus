using Domains;

namespace NobatPlusDATA.Domain
{
    public class AdminAuditLog : BaseEntity
    {
        public long ActorPersonID { get; set; }
        public Person? ActorPerson { get; set; }
        public string ActorFullName { get; set; } = "";
        public string ActionName { get; set; } = "";
        public string ControllerName { get; set; } = "";
        public string EntityName { get; set; } = "";
        public string HttpMethod { get; set; } = "";
        public string RequestPath { get; set; } = "";
        public string? TargetId { get; set; }
        public string? RequestSummary { get; set; }
        public int StatusCode { get; set; }
        public bool Succeeded { get; set; }
        public string? ErrorMessage { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
