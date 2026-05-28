namespace NobatPlusAPI.Tools
{
    public class CorsSettings
    {
        public bool usecors { get; set; }
        public int cookiesecurity { get; set; }
        public List<string> allowedOrigins { get; set; }
        public bool useRateLimiter { get; set; }
    }
    public class PayGatewaySettings
    {
        public string GatewayName { get; set; }
        public string MerchantId { get; set; }
        public bool IsSandbox { get; set; }
        public bool IsActive { get; set; }
    }
}
