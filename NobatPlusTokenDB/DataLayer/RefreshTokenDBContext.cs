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
        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}