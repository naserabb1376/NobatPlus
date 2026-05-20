namespace NobatPlusAPI.Models.AdminDashboard
{
    public class GetAdminDashboardReportRequestBody
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public long CityId { get; set; }
        public long RoleId { get; set; }
    }
}
