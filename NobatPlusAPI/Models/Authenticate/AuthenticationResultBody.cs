using Newtonsoft.Json;

namespace NobatPlusAPI.Models.Authenticate
{
    public class AuthenticationResultBody
    {
        public long PersonId { get; set; }
        public long CustomerId { get; set; }
        public long StylistId { get; set; }
        public long RoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [JsonIgnore]
        public string AccessToken { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }
        public bool IsActive { get; set; }
    }
}
