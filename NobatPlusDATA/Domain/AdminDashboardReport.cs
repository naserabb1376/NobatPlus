namespace NobatPlusDATA.Domain
{
    public class AdminDashboardReport
    {
        public AdminDashboardSummary Summary { get; set; } = new();
        public List<ChartPointDto> BookingTrend { get; set; } = new();
        public List<ChartPointDto> RevenueTrend { get; set; } = new();
        public List<NameValueDto> BookingStatusBreakdown { get; set; } = new();
        public List<SalonPerformanceDto> SalonPerformance { get; set; } = new();
        public List<StylistPerformanceDto> StylistPerformance { get; set; } = new();
        public List<ServicePerformanceDto> ServicePerformance { get; set; } = new();
        public List<CustomerPerformanceDto> CustomerPerformance { get; set; } = new();
        public List<CityPerformanceDto> CityPerformance { get; set; } = new();
        public List<SystemHealthDto> SystemHealth { get; set; } = new();
    }

    public class AdminDashboardSummary
    {
        public int TotalUsersCount { get; set; }
        public int ActiveUsersCount { get; set; }
        public int InactiveUsersCount { get; set; }
        public int CustomersCount { get; set; }
        public int SalonsCount { get; set; }
        public int StylistsCount { get; set; }
        public int PendingStylistsCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public int TodayBookingsCount { get; set; }
        public int CompletedBookingsCount { get; set; }
        public int CancelledBookingsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public decimal StylistAmount { get; set; }
        public decimal PlatformAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public int FinishedPaymentsCount { get; set; }
        public int UnfinishedPaymentsCount { get; set; }
        public int PendingSettlementRequestsCount { get; set; }
        public decimal PendingSettlementAmount { get; set; }
        public int PaidSettlementRequestsCount { get; set; }
        public decimal PaidSettlementAmount { get; set; }
        public int PendingDocumentsCount { get; set; }
        public int ApprovedDocumentsCount { get; set; }
        public int RejectedDocumentsCount { get; set; }
        public float AverageRating { get; set; }
        public double CancellationPercent { get; set; }
    }

    public class SalonPerformanceDto
    {
        public long SalonId { get; set; }
        public string SalonName { get; set; } = "";
        public int StylistsCount { get; set; }
        public int BookingCount { get; set; }
        public int CustomerCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public float AverageRating { get; set; }
        public double CancellationPercent { get; set; }
    }

    public class CityPerformanceDto
    {
        public long CityId { get; set; }
        public string CityName { get; set; } = "";
        public int UsersCount { get; set; }
        public int BookingsCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SystemHealthDto
    {
        public string Title { get; set; } = "";
        public int Count { get; set; }
        public string Severity { get; set; } = "";
    }
}
