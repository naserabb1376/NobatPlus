using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class JobType : BaseEntity
    {
        public string JobTitle { get; set; }
        public int SexTypeChecked { get; set; }

        public ICollection<Stylist> Stylists { get; set; }
    }
}