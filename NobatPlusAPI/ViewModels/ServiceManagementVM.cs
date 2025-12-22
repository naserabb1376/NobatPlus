using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class ServiceManagementVM : BaseEntity
    {
        public long ServiceParentID { get; set; }
        public string ServiceName { get; set; }
        public char ServiceGender { get; set; }
        public int StylistCount { get; set; }

    }
}