using Newtonsoft.Json;

namespace NobatPlusAPI.Models.Authenticate
{
    public class AuthenticationResultBody
    {
        public long PersonId { get; set; }
        public long CustomerId { get; set; }
        public long StylistId { get; set; }
        public long SalonId { get; set; }
        public long ActiveProfileId { get; set; }
        public string ActiveProfileType { get; set; } = "";
        public List<AuthenticationProfileBody> Profiles { get; set; } = new();
        public long RoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [JsonIgnore]
        public string AccessToken { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }
        public bool IsActive { get; set; }
    }

    public class AuthenticationProfileBody
    {
        public long ID { get; set; }
        public string ProfileType { get; set; } = "";
        public string Name { get; set; } = "";
        public long ParentId { get; set; }
        public bool IsActive { get; set; }
        public string AccountStatus { get; set; } = "";
    }
}
