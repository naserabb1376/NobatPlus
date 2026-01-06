using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class StylistServiceVM
    {
        public long StylistId { get; set; }
        public long ServiceManagementId { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public string ServiceTitle { get; set; }
        public decimal ServicePrice { get; set; }
        public TimeSpan ServiceDuration { get; set; }
        public int DepositPercent { get; set; }

    }
}