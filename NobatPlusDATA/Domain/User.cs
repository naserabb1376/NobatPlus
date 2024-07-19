using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;
using Microsoft.AspNetCore.Identity;

namespace Domain
{
    internal class User : BaseEntity
    {
        [Display(Name = "نام")]
        [MinLength(500)]
        public string FirstName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [MinLength(500)]
        public string LasttName { get; set; }

        [Display(Name = "نام کاربری")]
        [MinLength(500)]
        public string UserName { get; set; }

        [Display(Name = "نام خانوادگی")]
        [MinLength(500)]
        public string MobileNo { get; set; }

        [Display(Name = "ایمیل")]
        [MinLength(500)]
        public string? Email { get; set; }

        [Display(Name = "رمز")]
        [MinLength(500)]
        public string PasswordHash { get; set; }

        [Display(Name = "مهر امنیتی")]
        [MinLength(500)]
        public string SecurityStamp { get; set; }

        [Display(Name = "مهر همزمانی")]
        [MinLength(500)]
        public string ConcurrencyStamp { get; set; }

        [Display(Name = "ورود دو مرحله")]
        [MinLength(500)]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "تاریخ قفل شدن")]
        [MinLength(500)]
        public DateTime? LockoutEnd { get; set; }

        [Display(Name = "قفل بودن")]
        [MinLength(500)]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "تعداد ورود اشتباه")]
        [MinLength(500)]
        public int AccessFailedCount { get; set; }
    }

    public class UserRole : IdentityRole<int>
    {
    }
}