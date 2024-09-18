using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.DataLayer
{
    public class RefreshTokenDBContext : DbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // تنظیمات دیتابیس
            optionsBuilder.UseSqlServer("Server=your_server;Database=RefreshTokenDB;Trusted_Connection=True;");
        }
    }
}