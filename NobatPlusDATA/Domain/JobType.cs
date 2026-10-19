using Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class JobType : BaseEntity
    {
        public long JobTypeParentID { get; set; }
        public string JobTitle { get; set; }
        public int SexTypeChecked { get; set; }

        public long RoleId { get; set; } // 1.User 2.Worker 3.WorkShop 4.Admin

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        public ICollection<Stylist> Stylists { get; set; }
    }
}