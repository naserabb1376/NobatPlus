using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace NobatPlusDATA.Domain
{
    public class Login : BaseEntity
    {
        public long PersonID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLoginDate { get; set; }

        public Person Person { get; set; }
    }
}