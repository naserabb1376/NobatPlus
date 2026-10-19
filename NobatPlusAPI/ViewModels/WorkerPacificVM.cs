using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class WorkerPacificVM : BaseEntity
    {
        public long WorkerId { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
        public DateTime PacificStartDate { get; set; }
        public DateTime PacificEndDate { get; set; }
    }
}
