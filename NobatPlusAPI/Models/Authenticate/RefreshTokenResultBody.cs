using Newtonsoft.Json;

namespace NobatPlusAPI.Models.Authenticate
{
    public class RefreshTokenResultBody
    {
        [JsonIgnore]
        public string RefreshToken { get; set; }
        [JsonIgnore]
        public string AccessToken { get; set; }
    }
}
