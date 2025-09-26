using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.ViewModels
{
    public class NotificationVM : BaseEntity
    {
        public long PersonID { get; set; }
        public string PersonFullName { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }


    }
}