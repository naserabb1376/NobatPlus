using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain;

namespace NobatPlusTokenDB.DataLayer
{
    public class RefreshTokenDBContext : DbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configHelper = new ConfigurationHelper();
            string _connectionString = configHelper.GetConnectionString("Tokenpublicdb");
            optionsBuilder.UseSqlServer(_connectionString);
            // تنظیمات دیتابیس
        }
    }
}