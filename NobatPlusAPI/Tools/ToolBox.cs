using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using SixLabors.ImageSharp.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VerifyCodeSMSService;
using static NobatPlusAPI.Tools.ApiCaller;
using Path = System.IO.Path;

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

        public async static Task<bool> SendSMSMessage(string mobileNumber,string message)
        {
            string AppName = Configuration["Jwt:Issuer"];
            string UserName = Configuration["VerifyCode:PanelUserName"];
            string Password = Configuration["VerifyCode:PanelPassword"];
            string lineNumber = Configuration["VerifyCode:PanelLineNumber"];
            string apiUrl = $"https://RayganSMS.com/SendMessageWithUrl.ashx?Username={UserName}&Password={Password}&PhoneNumber={lineNumber}&MessageBody={message}&RecNumber={mobileNumber}&Smsclass=1";

                     List<ReqHeader> reqHeaders = new List<ReqHeader>();
           
            bool send = false;
            try
            {
                ApiCaller apiCaller = new ApiCaller();

                var SendSmsMessageResponse = await apiCaller.Call<long>(apiUrl, "GET","",reqHeaders,Encoding.UTF8);
                if (SendSmsMessageResponse > 2000) send = true;
                else send = false;

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

        // تابع تولید Access Token (JWT)
        public static string GenerateAccessToken(Login login)
        {
            var key = Configuration["Jwt:Key"];
            var issuer = Configuration["Jwt:Issuer"];
            var audience = Configuration["Jwt:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
            new Claim(JwtRegisteredClaimNames.Sub, login.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", login.PersonID.ToString()),
            new Claim("firstName", login.Person.FirstName),
            new Claim("lastName", login.Person.LastName),
            new Claim("Role", login.Person.RoleId.ToString())
        };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: signingCredentials);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }


        public static bool ValidateCaptcha(this string enteredCode,string storedCode)
        {
            bool isValidCaptcha = true;
            bool useCaptha = bool.Parse(Configuration["UseCaptcha"].ToString());
            if (useCaptha)
            {
                isValidCaptcha = (storedCode != null && enteredCode == storedCode);

            }

            return isValidCaptcha;
        }

        private static string GenerateResetPasswordToken(long loginId)
        {
            byte[] randomBytes = new byte[10]; // اندازه توکن را می‌توانید تغییر دهید
            using (var rngCryptoServiceProvider = new RNGCryptoServiceProvider())
            {
                rngCryptoServiceProvider.GetBytes(randomBytes);
            }
            string rawToken = GenerateRefreshToken();
            string theToken = $"{loginId}-{rawToken}";
            return theToken;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber); // تولید و بازگرداندن رفرش توکن
            }
        }

        public static string GenerateToken(long loginId = 0)
        {
            string token = "";
           if(loginId > 0)
            {
                token = GenerateResetPasswordToken(loginId);
            }
           else
            {
                token = GenerateRefreshToken();
            }
           return token;
        }

       

        public static bool SendEmail(string emailAddress, string subject, string body)
        {
            var issuer = Configuration["Jwt:Issuer"];
            var SmtpClient = Configuration["EmailSender:SmtpClient"];
            var emailEnable = bool.Parse(Configuration["EmailSender:Enable"]);
            var HostEmail = Configuration["EmailSender:HostEmail"];
            var EmailPass = Configuration["EmailSender:EmailPass"];
            bool send = !emailEnable;
            try
            {
                MailMessage mail = new MailMessage(
                    new MailAddress(HostEmail, issuer),
                    new MailAddress(emailAddress));
                mail.Subject = $"{subject} {issuer}";
                mail.Body = body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient(SmtpClient);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(HostEmail, EmailPass);
                smtp.Port = 25;
                smtp.EnableSsl = true;
                if (emailEnable)
                {
                    smtp.Send(mail);
                    send = true;
                }
            }
            catch (Exception ex)
            {
                SaveLog(ex.Message + '\n' + ex.InnerException?.Message);
                send = false;
            }
            return send;
        }

        public static string MakeResetPasswordMessage(string fullName, string token)
        {
            var issuer = Configuration["Jwt:Issuer"];
            var Audience = Configuration["Jwt:Audience"];
            var link = $"{Audience}?token={token}";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{fullName} عزیز! <br />");
            sb.AppendLine($"برای بازنشانی کامه عبور خود روی لینک زیر کلیک کنید <br />");
            sb.AppendLine($"<a href=\"{link}\" target=\"_parent\">بازنشانی کلمه عبور</a><br />");
            sb.AppendLine($"یا لینک زیر را داخل نوار آدرس مرورگر قرار دهید <br />");
            sb.AppendLine($"{link}<br />");
            sb.AppendLine($"گفتنی است که این لینک در طی دو ساعت آینده منقضی خواهد شد <br />");
            sb.AppendLine($"{issuer}<br />");
            return sb.ToString();
        }

        public static void SaveLog(object log)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{log.ToString()}");
            sb.AppendLine(DateTime.Now.ToShortTimeString());
            sb.AppendLine($"--------------------------------");
            var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
            File.AppendAllText(Path.Combine(logFilePath), sb.ToString());
        }

        private static string ApiVersion { get; set; }
        public static string CalculateAppVersionNo()
        {
            if (string.IsNullOrEmpty(ApiVersion))
            {
                string versionNo = Configuration["AppVersionNo"]?.ToString();

                if (string.IsNullOrEmpty(versionNo))
                {
                    string nowDate = DateTime.Now.ToShamsi().DateToString().Split(' ')[0];
                    var dateParts = nowDate.Substring(3).Split("/");

                    for (int i = 0; i < dateParts.Length; i++)
                    {
                        if (dateParts[i].StartsWith("0"))
                        {
                            dateParts[i] = dateParts[i].TrimStart('0');
                        }
                    }

                    versionNo = string.Join('.', dateParts);
                }

                ApiVersion = versionNo;
            }

            return ApiVersion;
        }


    }
}
