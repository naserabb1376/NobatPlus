namespace NobatPlusDATA.ViewModels
{
    public class AdminMonitoringReportVM
    {
        public AdminMonitoringSummaryVM Summary { get; set; } = new();
        public List<AdminMonitoringIndicatorVM> Indicators { get; set; } = new();
        public List<AdminMonitoringAuditFailureVM> RecentAuditFailures { get; set; } = new();
        public List<AdminMonitoringPaymentFailureVM> RecentPaymentFailures { get; set; } = new();
        public List<AdminMonitoringSmsFailureVM> RecentSmsFailures { get; set; } = new();
        public List<AdminMonitoringTicketVM> OpenTickets { get; set; } = new();
    }

    public class AdminMonitoringSummaryVM
    {
        public int FailedAdminOperations { get; set; }
        public int FailedPayments { get; set; }
        public decimal FailedPaymentAmount { get; set; }
        public int FailedSmsMessages { get; set; }
        public int OpenSupportTickets { get; set; }
        public int UrgentSupportTickets { get; set; }
        public int PendingDocuments { get; set; }
        public int PendingSettlements { get; set; }
        public decimal PendingSettlementAmount { get; set; }
        public int RescheduleRequiredBookings { get; set; }
        public int UnfinishedPayments { get; set; }
    }

    public class AdminMonitoringIndicatorVM
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Severity { get; set; } = "info";
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class AdminMonitoringAuditFailureVM
    {
        public long ID { get; set; }
        public string ActorFullName { get; set; } = "";
        public string ActionName { get; set; } = "";
        public string EntityName { get; set; } = "";
        public string RequestPath { get; set; } = "";
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; } = "";
        public DateTime OccurredAt { get; set; }
    }

    public class AdminMonitoringPaymentFailureVM
    {
        public long ID { get; set; }
        public long PaymentID { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string GatewayName { get; set; } = "";
        public string GatewayMessage { get; set; } = "";
        public string TrackingNumber { get; set; } = "";
    }

    public class AdminMonitoringSmsFailureVM
    {
        public long ID { get; set; }
        public string PhoneNumber { get; set; } = "";
        public string PersonFullName { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime SentDate { get; set; }
    }

    public class AdminMonitoringTicketVM
    {
        public long ID { get; set; }
        public string Title { get; set; } = "";
        public string PersonFullName { get; set; } = "";
        public string PersonPhoneNumber { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime LastMessageAt { get; set; }
    }
}
