using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Notification : BaseEntity
    {
        public long PersonID { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }

        public Person Person { get; set; }
    }
}