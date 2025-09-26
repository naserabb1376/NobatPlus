using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.ViewModels
{
    public class LoginVM : BaseEntity
    {
        public long PersonID { get; set; }
        public string PersonFullName { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLoginDate { get; set; }


    }
}