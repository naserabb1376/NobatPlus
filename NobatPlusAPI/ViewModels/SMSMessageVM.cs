using Domains;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.ViewModels
{
    public class SMSMessageVM : BaseEntity
    {
        public long PersonID { get; set; }
        public string PersonFullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }
        public bool SentStatus { get; set; }


    }
}