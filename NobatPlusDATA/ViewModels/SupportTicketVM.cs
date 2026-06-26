namespace NobatPlusDATA.ViewModels
{
    public class SupportTicketVM
    {
        public long ID { get; set; }
        public long PersonID { get; set; }
        public string PersonFullName { get; set; } = "";
        public string PersonPhoneNumber { get; set; } = "";
        public string PersonEmail { get; set; } = "";
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Status { get; set; } = "";
        public long? AssignedAdminPersonID { get; set; }
        public string AssignedAdminName { get; set; } = "";
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string Description { get; set; } = "";
        public int MessageCount { get; set; }
        public string LastMessage { get; set; } = "";
        public List<SupportTicketMessageVM> Messages { get; set; } = new();
    }

    public class SupportTicketMessageVM
    {
        public long ID { get; set; }
        public long SupportTicketID { get; set; }
        public long SenderPersonID { get; set; }
        public string SenderFullName { get; set; } = "";
        public string SenderPhoneNumber { get; set; } = "";
        public bool IsAdminReply { get; set; }
        public string Message { get; set; } = "";
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Description { get; set; } = "";
    }
}
