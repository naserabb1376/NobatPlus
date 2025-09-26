using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.ViewModels
{
    public class CustomerVM : BaseEntity
    {
        public long PersonID { get; set; }
        public string PersonFullName { get; set; }

    }
}