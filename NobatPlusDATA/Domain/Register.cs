using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Register : BaseEntity
    {
        public int PersonID { get; set; }
        public DateTime RegistrationDate { get; set; }

        public Person Person { get; set; }
    }
}