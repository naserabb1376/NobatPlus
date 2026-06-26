using Domains;

namespace NobatPlusDATA.Domain
{
    public class SupportTicket : BaseEntity
    {
        public long PersonID { get; set; }
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Priority { get; set; } = "normal";
        public string Status { get; set; } = "open";
        public long? AssignedAdminPersonID { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        public Person Person { get; set; }
        public Person? AssignedAdminPerson { get; set; }
        public ICollection<SupportTicketMessage> Messages { get; set; } = new List<SupportTicketMessage>();
    }

    public class SupportTicketMessage : BaseEntity
    {
        public long SupportTicketID { get; set; }
        public long SenderPersonID { get; set; }
        public bool IsAdminReply { get; set; }
        public string Message { get; set; } = "";

        public SupportTicket SupportTicket { get; set; }
        public Person SenderPerson { get; set; }
    }
}
