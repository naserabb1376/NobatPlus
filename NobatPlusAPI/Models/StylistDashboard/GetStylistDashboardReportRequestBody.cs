namespace NobatPlusAPI.Models.StylistDashboard
{
    public class GetStylistDashboardReportRequestBody
    {
        public long StylistId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
