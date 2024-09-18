using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Domain
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; } // شناسه کاربر
        public string Token { get; set; } // رفرش توکن
        public DateTime ExpiryDate { get; set; } // تاریخ انقضا
        public DateTime CreatedDate { get; set; } = DateTime.Now; // تاریخ ایجاد
        public DateTime? RevokedDate { get; set; } // تاریخ لغو
    }
}