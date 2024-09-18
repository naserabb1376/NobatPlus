using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class RefreshToken
    {
        public long ID { get; set; }
        public long UserId { get; set; } // شناسه کاربر
        public string Token { get; set; } // رفرش توکن
        public string Type { get; set; } // رفرش توکن
        public Boolean Status { get; set; } // رفرش توکن
        public DateTime ExpiryDate { get; set; } // تاریخ انقضا
        public DateTime CreatedDate { get; set; } = DateTime.Now; // تاریخ ایجاد
        public DateTime? RevokedDate { get; set; } // تاریخ لغو
    }
}