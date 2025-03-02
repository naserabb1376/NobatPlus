namespace NobatPlusAPI.Tools
{
    public class CorsSettings
    {
        public bool usecors { get; set; }
        public int cookiesecurity { get; set; }
        public List<string> allowedOrigins { get; set; }
        public bool useRateLimiter { get; set; }
    }
}
