namespace NobatPlusAPI.Models.WorkerDashboard
{
    public class GetWorkerDashboardReportRequestBody
    {
        public long WorkerId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
