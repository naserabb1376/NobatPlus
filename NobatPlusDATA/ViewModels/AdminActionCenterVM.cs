namespace NobatPlusDATA.ViewModels
{
    public class AdminActionCenterVM
    {
        public AdminActionCenterSummaryVM Summary { get; set; } = new();
        public List<AdminActionItemVM> Items { get; set; } = new();
    }

    public class AdminActionCenterSummaryVM
    {
        public int TotalCount { get; set; }
        public int DangerCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public int PendingDocumentsCount { get; set; }
        public int PendingSettlementsCount { get; set; }
        public int RescheduleBookingsCount { get; set; }
        public int UnfinishedPaymentsCount { get; set; }
        public int ProviderDataIssuesCount { get; set; }
        public int SupportIssuesCount { get; set; }
    }

    public class AdminActionItemVM
    {
        public string Type { get; set; } = "";
        public string Severity { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public long EntityID { get; set; }
        public string EntityName { get; set; } = "";
        public DateTime? CreatedAt { get; set; }
        public int AgeDays { get; set; }
        public decimal Amount { get; set; }
        public string ActionPath { get; set; } = "";
        public string ActionLabel { get; set; } = "";
    }
}
