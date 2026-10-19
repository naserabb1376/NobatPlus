using Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    //[NotMapped]   // ⛔ در دیتابیس ساخته نمی‌شود
    public class WorkerPacific : BaseEntity
    {
        public long WorkerID { get; set; }
        public DateTime PacificStartDate { get; set; }
        public DateTime PacificEndDate { get; set; }

        public Worker Worker { get; set; }
    }
}