using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class StylistPacific : BaseEntity
    {
        public long StylistID { get; set; }
        public DateTime PacificStartDate { get; set; }
        public DateTime PacificEndDate { get; set; }

        public Stylist Stylist { get; set; }
    }
}