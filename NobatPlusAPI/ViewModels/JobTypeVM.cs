using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class JobTypeVM: BaseEntity
    {
        public long JobTypeParntId { get; set; }
        public string JobTitle { get; set; }
        public long RoleId { get; set; } // 1.User 2.Worker 3.WorkShop 4.Admin
        public string RoleName { get; set; } // 1.User 2.Worker 3.WorkShop 4.Admin
        public int SexTypeChecked { get; set; }

    }
}