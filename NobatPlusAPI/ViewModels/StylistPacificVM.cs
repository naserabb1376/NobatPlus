using Domains;
using NobatPlusDATA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class StylistPacificVM : BaseEntity
    {
        public long StylistId { get; set; }
        public string StylistName { get; set; }
        public string SalonName { get; set; }
        public DateTime PacificStartDate { get; set; }
        public DateTime PacificEndDate { get; set; }

    }
}