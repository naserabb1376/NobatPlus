using Domain;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NobatPlusDATA.ViewModels
{
    public class PersonVM : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Gender { get; set; }
        public int RoleId { get; set; } // 1. User 2.Stylist 3. Salon
        public long? AddressID { get; set; }
        public string? NaCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}