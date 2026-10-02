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
        public RefreshTokenDBContext(DbContextOptions<RefreshTokenDBContext> options)
      : base(options)
        {
        }
        //public RefreshTokenDBContext()
        //{
        //}
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    TokenDbConfigurationHelper configurationHelper = new TokenDbConfigurationHelper();
        //    optionsBuilder.UseSqlServer(configurationHelper.GetConnectionString("Tokenpublicdb"));
        //    //  base.OnConfiguring(optionsBuilder);
        //}
    }
}