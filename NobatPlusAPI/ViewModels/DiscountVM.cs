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

    }
}