using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NobatPlusDATA.Tools
{
    public class MainDbConfigurationHelper
    {
        public IConfigurationRoot Configuration { get; }

        public MainDbConfigurationHelper()
        {
            // Build the configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("DataSetting.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public string GetConnectionString(string name)
        {
            // Retrieve the connection string
            return Configuration.GetConnectionString(name);
        }
    }
}
