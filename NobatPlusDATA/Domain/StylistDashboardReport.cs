namespace NobatPlusDATA.Domain
{
    public class StylistDashboardReport
    {
        public StylistDashboardSummary Summary { get; set; } = new();
        public List<StylistDashboardBookingDto> TodayBookings { get; set; } = new();
        public List<StylistDashboardBookingDto> NewBookings { get; set; } = new();
        public StylistDashboardBookingDto? NextBooking { get; set; }
        public List<ChartPointDto> BookingTrend { get; set; } = new();
        public List<ChartPointDto> RevenueTrend { get; set; } = new();
        public List<NameValueDto> BookingStatusBreakdown { get; set; } = new();
        public List<ServicePerformanceDto> ServicePerformance { get; set; } = new();
        public List<CustomerPerformanceDto> CustomerPerformance { get; set; } = new();
        public List<NameValueDto> RatingDistribution { get; set; } = new();
        public List<RatingQuestionDto> RatingQuestions { get; set; } = new();
        public List<ChartPointDto> CancellationTrend { get; set; } = new();
        public List<NameValueDto> CancellationReasons { get; set; } = new();
        public List<WorkTimeCapacityDto> WorkTimeCapacity { get; set; } = new();
        public List<NameValueDto> DiscountBreakdown { get; set; } = new();
    }

    public class SalonDashboardReport : StylistDashboardReport
    {
        public List<StylistPerformanceDto> StylistPerformance { get; set; } = new();
    }

    public class StylistDashboardSummary
    {
        public int TodayBookingsCount { get; set; }
        public int NewBookingsCount { get; set; }
        public int TotalBookingsCount { get; set; }
        public int CompletedBookingsCount { get; set; }
        public int CancelledBookingsCount { get; set; }
        public int TotalCustomersCount { get; set; }
        public int NewCustomersCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public decimal StylistAmount { get; set; }
        public decimal PlatformAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public float AverageRating { get; set; }
        public double RecommendPercent { get; set; }
        public int ReviewCount { get; set; }
        public double CancellationPercent { get; set; }
        public double CapacityUsagePercent { get; set; }
    }

    public class ChartPointDto
    {
        public string Label { get; set; } = "";
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }

    public class StylistDashboardBookingDto
    {
        public long BookingId { get; set; }
        public long CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhoneNumber { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public string BookingDateText { get; set; } = "";
        public string BookingTime { get; set; } = "";
        public string Services { get; set; } = "";
        public List<long> ServiceIds { get; set; } = new();
        public string Status { get; set; } = "";
        public bool IsCancelled { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public decimal StylistAmount { get; set; }
    }

    public class NameValueDto
    {
        public string Name { get; set; } = "";
        public decimal Value { get; set; }
        public int Count { get; set; }
    }

    public class ServicePerformanceDto
    {
        public long ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }

    public class CustomerPerformanceDto
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public int BookingCount { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public decimal PaidAmount { get; set; }
    }

    public class StylistPerformanceDto
    {
        public long StylistId { get; set; }
        public string StylistName { get; set; } = "";
        public int BookingCount { get; set; }
        public int CompletedBookingCount { get; set; }
        public int CancelledBookingCount { get; set; }
        public int CustomerCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public float AverageRating { get; set; }
        public double CancellationPercent { get; set; }
    }

    public class RatingQuestionDto
    {
        public long RateQuestionId { get; set; }
        public string QuestionText { get; set; } = "";
        public float AverageScore { get; set; }
        public int Count { get; set; }
    }

    public class WorkTimeCapacityDto
    {
        public string DayOfWeek { get; set; } = "";
        public double WorkHours { get; set; }
        public int BookingCount { get; set; }
        public int BookedMinutes { get; set; }
        public double CapacityUsagePercent { get; set; }
    }
}
