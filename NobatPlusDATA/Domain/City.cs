using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class City : BaseEntity
    {
        public int CityParentID { get; set; }
        public string CityName { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}