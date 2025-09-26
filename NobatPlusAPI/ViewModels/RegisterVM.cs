using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.ViewModels
{
    public class RegisterVM : BaseEntity
    {
        public long PersonID { get; set; }
        public string PersonFullName { get; set; }
        public DateTime RegistrationDate { get; set; }


    }
}