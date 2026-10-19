using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class DiscountAssignmentVM:BaseEntity
    {
        public long DiscountId { get; set; }
        public long? AdminId { get; set; }
        public long? WorkerId { get; set; }
        public string DiscountCode { get; set; }
        public int DiscountAmount { get; set; }
        public string AdminFullName { get; set; }
        public string WorkerName { get; set; }
        public string WorkshopName { get; set; }
    }
}