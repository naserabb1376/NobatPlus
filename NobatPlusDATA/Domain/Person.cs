using Domain;
using Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class Person : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Gender { get; set; }

       // [NotMapped]   // ⛔ در دیتابیس ساخته نمی‌شود
        public long RoleId { get; set; } // 1.User 2.Stylist 3.Salon 4.Admin

       // [NotMapped]   // ⛔ در دیتابیس ساخته نمی‌شود
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
        public long? AddressID { get; set; }
        public Address? Address { get; set; }
        public string? NaCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateOfBirth { get; set; }

        public ICollection<Notification> Notifications { get; set; }
        public ICollection<SMSMessage> SMSMessages { get; set; }
    }
}