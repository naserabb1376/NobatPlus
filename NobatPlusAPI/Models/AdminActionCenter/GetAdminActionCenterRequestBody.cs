namespace NobatPlusAPI.Models.AdminActionCenter
{
    public class GetAdminActionCenterRequestBody
    {
        public string Type { get; set; } = "";
        public string Severity { get; set; } = "";
        public int MaxItemsPerType { get; set; } = 20;
    }
}
