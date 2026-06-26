using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class DiscountVM:BaseEntity
    {
        public string DiscountCode { get; set; }
        public int DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool CodeRequired { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public int AssignmentCount { get; set; }
        public int CustomerCount { get; set; }
        public int ServiceCount { get; set; }
        public List<long> StylistIds { get; set; } = new();
        public List<long> CustomerIds { get; set; } = new();
        public List<long> ServiceIds { get; set; } = new();

    }
}
