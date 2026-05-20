namespace NobatPlusAPI.Models.CustomerDashboard
{
    public class GetCustomerDashboardReportRequestBody
    {
        public long PersonId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
