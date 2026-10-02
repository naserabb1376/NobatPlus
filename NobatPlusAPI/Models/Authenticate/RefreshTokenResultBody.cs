using Newtonsoft.Json;

namespace NobatPlusAPI.Models.Authenticate
{
    public class RefreshTokenResultBody
    {
        [JsonIgnore]
        public string RefreshToken { get; set; }
        [JsonIgnore]
        public string AccessToken { get; set; }
        public long RoleId { get; set; }
        public long StylistId { get; set; }
        public long SalonId { get; set; }
        public long ActiveProfileId { get; set; }
        public string ActiveProfileType { get; set; } = "";
        public List<AuthenticationProfileBody> Profiles { get; set; } = new();
    }
}
