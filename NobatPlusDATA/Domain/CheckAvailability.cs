using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class CheckAvailability : BaseEntity
    {
        public long WorkerID { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }

        public Worker Worker { get; set; }
    }
}