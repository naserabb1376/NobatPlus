using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NobatPlusDATA.Tools;

namespace NobatPlusDATA.DataLayer
{
    public class NobatPlusContextFactory : IDesignTimeDbContextFactory<NobatPlusContext>
    {
        public NobatPlusContext CreateDbContext(string[] args)
        {
            var configurationHelper = new MainDbConfigurationHelper();
            var optionsBuilder = new DbContextOptionsBuilder<NobatPlusContext>();

            optionsBuilder.UseSqlServer(configurationHelper.GetConnectionString("publicdb"));

            return new NobatPlusContext(optionsBuilder.Options);
        }
    }
}
