using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusAPI.ViewModels
{
    public class AdminVM : BaseEntity
    {
        public long PersonID { get; set; }
        public string Role { get; set; }

        public string PersonFullName { get; set; }
    }
}