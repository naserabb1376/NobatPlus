using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class CityVM:BaseEntity
    {
        public long CityParentID { get; set; }
        public string CityName { get; set; }

    }
}