using System.Text;
using VerifyCodeSMSService;

namespace NobatPlusAPI.Tools
{
    public static class ToolBox
    {
        private static IConfigurationRoot Configuration { get; }

        static ToolBox()
        {
            var builder = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
          Configuration = builder.Build();
        }
        public async static Task<bool> SendCode(string mobileNumber)
        {
            string AppName = Configuration["Jwt:Issuer"];
            string UserName = Configuration["VerifyCode:PanelUserName"];
            string Password = Configuration["VerifyCode:PanelPassword"];

            bool send = false;
            try
            {
                AutoSendCodeResponse x = null;
                using (FastSendSoapClient client = new FastSendSoapClient(FastSendSoapClient.EndpointConfiguration.FastSendSoap))
                {
                    x = await client.AutoSendCodeAsync(UserName, Password, mobileNumber, AppName);
                    send = true;
                }
            }
            catch (Exception ex)
            {
                send = false;
            }
            return send;
        }

        public async static Task<bool> CheckCode(string mobileNumber, string code)
        {
            string UserName = Configuration["VerifyCode:PanelUserName"];
            string Password = Configuration["VerifyCode:PanelPassword"];
            bool currect = false;
            try
            {
                using (FastSendSoapClient client = new FastSendSoapClient(FastSendSoapClient.EndpointConfiguration.FastSendSoap))
                {
                    CheckSendCodeResponse response = await client.CheckSendCodeAsync(UserName, Password, mobileNumber, code);
                    currect = response.Body.CheckSendCodeResult;
                }
            }
            catch (Exception)
            {
                currect = false;
            }
            return currect;
        }

        public static void SaveLog(object log)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{log.ToString()}");
            sb.AppendLine(DateTime.Now.ToShortTimeString());
            sb.AppendLine($"--------------------------------");
            System.IO.File.AppendAllText(Path.Combine("log.txt"), sb.ToString(),Encoding.UTF8);
        }

    }
}
