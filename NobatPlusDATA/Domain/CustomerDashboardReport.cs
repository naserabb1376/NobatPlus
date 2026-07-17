namespace NobatPlusDATA.Domain
{
    public class CustomerDashboardReport
    {
        public CustomerDashboardSummary Summary { get; set; } = new();
        public List<CustomerAppointmentDto> TodayAppointments { get; set; } = new();
        public List<CustomerAppointmentDto> UpcomingAppointments { get; set; } = new();
        public List<CustomerPaymentDto> RecentPayments { get; set; } = new();
        public List<CustomerServiceHistoryDto> RecentServices { get; set; } = new();
        public List<CustomerStylistHistoryDto> RecentStylists { get; set; } = new();
        public List<CustomerPendingReviewDto> PendingReviews { get; set; } = new();
        public List<ChartPointDto> BookingTrend { get; set; } = new();
    }

    public class CustomerDashboardSummary
    {
        public int TodayAppointmentsCount { get; set; }
        public int UpcomingAppointmentsCount { get; set; }
        public int CompletedAppointmentsCount { get; set; }
        public int CancelledAppointmentsCount { get; set; }
        public int ActiveDiscountsCount { get; set; }
        public int UnreadNotificationsCount { get; set; }
        public int PendingReviewsCount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public decimal MonthPaidAmount { get; set; }
    }

    public class CustomerAppointmentDto
    {
        public long BookingId { get; set; }
        public string Date { get; set; } = "";
        public DateTime BookingDate { get; set; }
        public string Time { get; set; } = "";
        public string Services { get; set; } = "";
        public string StylistName { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public string Status { get; set; } = "";
        public bool IsCancelled { get; set; }
    }

    public class CustomerPaymentDto
    {
        public long PaymentId { get; set; }
        public long BookingId { get; set; }
        public string Date { get; set; } = "";
        public DateTime PaymentDate { get; set; }
        public string Services { get; set; } = "";
        public string StylistName { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainAmount { get; set; }
        public string Status { get; set; } = "";
    }

    public class CustomerServiceHistoryDto
    {
        public long ServiceId { get; set; }
        public string ServiceName { get; set; } = "";
        public int BookingCount { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public string LastBookingDateText { get; set; } = "";
        public decimal PaidAmount { get; set; }
    }

    public class CustomerStylistHistoryDto
    {
        public long StylistId { get; set; }
        public string StylistName { get; set; } = "";
        public int BookingCount { get; set; }
        public DateTime? LastBookingDate { get; set; }
        public string LastBookingDateText { get; set; } = "";
        public decimal PaidAmount { get; set; }
    }

    public class CustomerPendingReviewDto
    {
        public long BookingId { get; set; }
        public long CustomerID { get; set; }
        public long StylistID { get; set; }
        public string Date { get; set; } = "";
        public string Services { get; set; } = "";
        public string StylistName { get; set; } = "";
    }
}
