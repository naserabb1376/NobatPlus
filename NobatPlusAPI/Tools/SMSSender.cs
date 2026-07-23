using Azure.Core;
using Newtonsoft.Json;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.ViewModels;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Reflection.PortableExecutable;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml;
using VerifyCodeSMSService;
using static NobatPlusAPI.Tools.ApiCaller;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace NobatPlusAPI.Tools
{

    public interface ISMSSender
    {
        public Task<bool> SendMessage(string mobileNumber,string message);
        public Task<VerifyCodeResult> SendCode(string mobileNumber);
        public Task<bool> CheckCode(string mobileNumber, string code);
    }
    public class RayganSMSSender : ISMSSender
    {
        private IConfigurationRoot Configuration { get; }

        string PanelUserName, PanelPassword, PanelLineNumber, PanelApiUrl, PanelApiKey, AppName;
        bool GenerateVerifyCode;

        public RayganSMSSender()
        {
            var builder = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            AppName = Configuration["Jwt:Issuer"];
            PanelUserName = Configuration["SmsSender:PanelUserName"];
            PanelPassword = Configuration["SmsSender:PanelPassword"];
            PanelApiKey = Configuration["SmsSender:PanelApiKey"];
            PanelApiUrl = Configuration["SmsSender:PanelApiUrl"];
            PanelLineNumber = Configuration["SmsSender:PanelLineNumber"];
            GenerateVerifyCode = bool.Parse(Configuration["SmsSender:GenerateVerifyCode"]);
        }
        public async Task<bool> SendMessage(string mobileNumber, string message)
        {
            List<ReqHeader> reqHeaders = new List<ReqHeader>();

            string apiUrl = $"{PanelApiUrl}?Username={PanelUserName}&Password={PanelPassword}&PhoneNumber={PanelLineNumber}&MessageBody={message}&RecNumber={mobileNumber}&Smsclass=1";
            bool send = false;
            try
            {
                ApiCaller apiCaller = new ApiCaller();

                var SendSmsMessageResponse = await apiCaller.Call<long>(apiUrl, "GET", "", reqHeaders, Encoding.UTF8);
                if (SendSmsMessageResponse > 2000) send = true;
                else send = false;

            }
            catch (Exception ex)
            {
                send = false;
            }
            return send;
        }
      

        public async Task<VerifyCodeResult> SendCode(string mobileNumber)
        {
            var result = new VerifyCodeResult();
            string theCode = "";
            bool send = false;
            try
            {
                if (GenerateVerifyCode)
                {
                    theCode = GenerateVerifyCodeManualy();
                    string smsMessage = $@" کد تایید شما:
{theCode}
نوبتیکس
";
                    send = await SendMessage(mobileNumber, smsMessage);
                    if (send) OtpStore.Save(mobileNumber, theCode);
                }
                else
                {
                    AutoSendCodeResponse x = null;
                    using (FastSendSoapClient client = new FastSendSoapClient(FastSendSoapClient.EndpointConfiguration.FastSendSoap))
                    {
                        x = await client.AutoSendCodeAsync(PanelUserName, PanelPassword, mobileNumber, AppName);
                        send = true;
                    }
                }
            }
            catch (Exception ex)
            {
                send = false;
            }
            result.SendStatus = send;
            result.Code = theCode ?? "";
            return result;
        }

        public async Task<bool> CheckCode(string mobileNumber, string code)
        {
            bool currect = false;
            try
            {
                if (GenerateVerifyCode)
                {
                    currect = OtpStore.Verify(mobileNumber, code);
                }
                else
                {
                    using (FastSendSoapClient client = new FastSendSoapClient(FastSendSoapClient.EndpointConfiguration.FastSendSoap))
                    {
                        CheckSendCodeResponse response = await client.CheckSendCodeAsync(PanelUserName, PanelPassword, mobileNumber, code);
                        currect = response.Body.CheckSendCodeResult;
                    }
                }
            }
            catch (Exception)
            {
                currect = false;
            }
            return currect;
        }

        private string GenerateVerifyCodeManualy()
        {
            Random random = new Random();
            return random.Next(111111, 999999).ToString();
        }
    }

    public class FarazSMSSender : ISMSSender
    {
        private IConfigurationRoot Configuration { get; }

        string PanelUserName, PanelPassword, PanelLineNumber, PanelApiUrl, PanelApiKey, AppName;
        bool GenerateVerifyCode;

        public FarazSMSSender()
        {
            var builder = new ConfigurationBuilder()
.SetBasePath(Directory.GetCurrentDirectory())
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();

            AppName = Configuration["Jwt:Issuer"];
            PanelUserName = Configuration["SmsSender:PanelUserName"];
            PanelPassword = Configuration["SmsSender:PanelPassword"];
            PanelApiKey = Configuration["SmsSender:PanelApiKey"];
            PanelApiUrl = Configuration["SmsSender:PanelApiUrl"];
            PanelLineNumber = Configuration["SmsSender:PanelLineNumber"];
            GenerateVerifyCode = bool.Parse(Configuration["SmsSender:GenerateVerifyCode"]);
        }

        public async Task<bool> SendMessage(string mobileNumber, string message)
        {
            bool sent = false;
            try
            {
                var farazSmsWebServiceBody = new
                {
                    sending_type  = "webservice",
                    from_number = PanelLineNumber,
                    message = message,
                    Params = new
                    {
                        recipients = new List<string>() { mobileNumber }
                    }
                };

                var jsonBody = JsonConvert.SerializeObject(farazSmsWebServiceBody).ToLower();

                List<ReqHeader> reqHeaders = new List<ReqHeader>();

                reqHeaders.Add(new ReqHeader() { Key = "Authorization", Value = PanelApiKey });


                ApiCaller apiCaller = new ApiCaller();

                var SendSmsMessageResponse = await apiCaller.Call<object>($"{PanelApiUrl}/api/send", "POST", jsonBody, reqHeaders, Encoding.UTF8);

                if (SendSmsMessageResponse.ToString().ToLower().Contains("\"status\": true")) sent = true;
                else sent = false;

            }
            catch (Exception)
            {
                sent = false;
            }
            return sent;

           
        }


        public async Task<VerifyCodeResult> SendCode(string mobileNumber)
        {
            var result = new VerifyCodeResult();
            bool sent = false;

            try
            {
                var theCode = GenerateVerifyCodeManualy();

                var farazSmsPatternBody = new
                {
                    sending_type = "pattern",
                    from_number = PanelLineNumber,
                    code = "h1ptkaoijnbbce9",
                    recipients = new List<string>() { mobileNumber },
                    Params = new { ver = theCode }
                };
                var jsonBody = JsonConvert.SerializeObject(farazSmsPatternBody).ToLower();

                List<ReqHeader> reqHeaders = new List<ReqHeader>();

                reqHeaders.Add(new ReqHeader() { Key = "Authorization", Value = PanelApiKey });


                ApiCaller apiCaller = new ApiCaller();

                var SendSmsMessageResponse = await apiCaller.Call<object>($"{PanelApiUrl}/api/send", "POST", jsonBody, reqHeaders, Encoding.UTF8);

                if (SendSmsMessageResponse.ToString().ToLower().Contains("\"status\": true")) sent = true;
                else sent = false;

                if (sent) OtpStore.Save(mobileNumber, theCode);
                result.SendStatus = sent;
                result.Code = theCode ?? "";
            }
            catch (Exception)
            {
                sent = false;
            }
            return result;
        }

        public async Task<bool> CheckCode(string mobileNumber, string code)
        {
            bool currect = false;
            try
            {
                currect = OtpStore.Verify(mobileNumber, code);
            }
            catch (Exception)
            {
                currect = false;
            }
            return currect;
        }

        private string GenerateVerifyCodeManualy()
        {
            Random random = new Random();
            return random.Next(111111, 999999).ToString();
        }
    }
}
