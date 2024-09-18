using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.Models.Authenticate;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Azure.Core;

namespace NobatPlusAPI.Controllers
{
    [Route("authentication")]
    [ApiController]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILoginRep _loginRep;
        private readonly IRegisterRep _registerRep;
        private readonly IPersonRep _personRep;
        private readonly ICustomerRep _customerRep;
        private readonly IStylistRep _stylistRep;
        private readonly IAddressRep _addressRep;
        private readonly ILogRep _logRep;
        private readonly ITokenRep _tokenRep;

        public AuthenticationController(IConfiguration configuration,ILoginRep loginRep, IRegisterRep registerRep, IPersonRep personRep, ICustomerRep customerRep, IStylistRep stylistRep, IAddressRep addressRep,ILogRep logRep,ITokenRep tokenRep)
        {
            _configuration = configuration;
            _loginRep = loginRep;
            _registerRep = registerRep;
            _personRep = personRep;
            _customerRep = customerRep;
            _stylistRep = stylistRep;
            _addressRep = addressRep;
            _logRep = logRep;
            _tokenRep = tokenRep;
        }

        [HttpPost("Authenticate")]
        public async Task<ActionResult<RowResultObject<AuthenticationResultBody>>> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            RowResultObject<AuthenticationResultBody> result = new RowResultObject<AuthenticationResultBody>();
            RowResultObject<Login> authenticateResult = new RowResultObject<Login>();

            try
            {

                var storedCaptchaCode = HttpContext.Session.GetString("CaptchaCode");

                if ((storedCaptchaCode == null || authenticationRequestBody.CaptchaCode != storedCaptchaCode))
                {
                    result.Status = false;
                    result.ErrorMessage = "کد کپچا نادرست است.";
                    return BadRequest(result);
                }

                switch (authenticationRequestBody.LoginType)
                {
                    default:
                    case 1:
                        authenticateResult = await _loginRep.AuthenticateAsync(authenticationRequestBody.UserName, authenticationRequestBody.Password, authenticationRequestBody.LoginType);
                        break;
                    case 2:
                        var validPhoneNumber = await _loginRep.ExistLoginAsync(authenticationRequestBody.UserName, 3);
                        if (!validPhoneNumber.Status && string.IsNullOrEmpty(validPhoneNumber.ErrorMessage))
                        {
                            result.Status = validPhoneNumber.Status;
                            result.ErrorMessage = "شماره تماس نامعتبر است";
                            return BadRequest(result);
                        }
                        bool validCode = await ToolBox.CheckCode(authenticationRequestBody.UserName, authenticationRequestBody.Password);
                        if (validCode)
                        {
                            authenticateResult = await _loginRep.AuthenticateAsync(authenticationRequestBody.UserName, authenticationRequestBody.Password, authenticationRequestBody.LoginType);
                        }
                        else
                        {
                            result.Status = validCode;
                            result.ErrorMessage = "کد تایید نامعتبر است";
                            return BadRequest(result);
                        }
                        break;
                    case 3:
                        authenticateResult = await _loginRep.AuthenticateAsync(authenticationRequestBody.UserName, authenticationRequestBody.Password, authenticationRequestBody.LoginType);
                        break;
                }

                result.Status = authenticateResult.Status;
                result.ErrorMessage = authenticateResult.ErrorMessage;

                if (authenticateResult.Status)
                {
                    var refreshToken = ToolBox.GenerateToken(); // تولید رفرش توکن
                    var accessToken = ToolBox.GenerateAccessToken(authenticateResult.Result); // تولید رفرش توکن
                    var refreshTokenExpiryDate = DateTime.Now.ToShamsi().AddDays(30); // تنظیم تاریخ انقضای رفرش توکن برای 30 روز


                    var refreshTokenRecord = new RefreshToken
                    {
                        UserId = authenticateResult.Result.PersonID,
                        Token = refreshToken, // ذخیره رفرش توکن
                        Type = "RefreshToken", // نوع: RefreshToken
                        Status = true,
                        CreatedDate = DateTime.Now.ToShamsi(),
                        ExpiryDate = refreshTokenExpiryDate // تاریخ انقضا
                    };

                    var saverefreshToken = await _tokenRep.AddRefreshTokenAsync(refreshTokenRecord);

                    if (saverefreshToken.Status)
                    {
                        result.Status = authenticateResult.Status;
                        result.ErrorMessage = authenticateResult.ErrorMessage;
                        result.Result = new AuthenticationResultBody()
                        {
                            RefreshToken = refreshToken, // بازگرداندن رفرش توکن
                            AccessToken = accessToken, // بازگرداندن اکسس توکن
                            PersonId = authenticateResult.Result.PersonID,
                            FullName = $"{authenticateResult.Result.Person.FirstName} {authenticateResult.Result.Person.LastName}"
                        };

                        #region AddLog
                        Log log = new Log()
                        {
                            CreateDate = DateTime.Now.ToShamsi(),
                            UpdateDate = DateTime.Now.ToShamsi(),
                            LogTime = DateTime.Now.ToShamsi(),
                            ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
                        };
                        await _logRep.AddLogAsync(log);
                        #endregion

                        return Ok(result);
                    }

                    else
                    {
                        result.Status = saverefreshToken.Status;
                        result.ErrorMessage = saverefreshToken.ErrorMessage;
                    }
                }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message}\n{ex.InnerException?.Message}";
            }


            return BadRequest(result);
        }

        [HttpPost("GenerateCaptchaImage")]
        public async Task<ActionResult> GenerateCaptchaImage()
        {
            // ایجاد کد کپچا
            var captchaCode = Guid.NewGuid().ToString().Substring(0, 5);

            // ذخیره کد کپچا در session
            HttpContext.Session.SetString("CaptchaCode", captchaCode);

            // تنظیمات تصویر کپچا
            int width = 150;
            int height = 50;
            var font = SystemFonts.CreateFont("Arial", 25, FontStyle.Bold);
            using (var image = new Image<Rgba32>(width, height))
            {
                // رنگ پس زمینه
                image.Mutate(ctx => ctx.Fill(Color.White));

                // افزودن نویز به پس زمینه
                var random = new Random();
                for (int i = 0; i < 50; i++)
                {
                    image.Mutate(ctx => ctx.DrawLine(Color.LightGray, 1,
                        new PointF(random.Next(width), random.Next(height)),
                        new PointF(random.Next(width), random.Next(height))));
                }

                // افزودن متن کپچا
                image.Mutate(ctx => ctx.DrawText(captchaCode, font, Color.Black, new PointF(20, 10)));

                // افزودن نویز روی متن
                for (int i = 0; i < 10; i++)
                {
                    image.Mutate(ctx => ctx.DrawLine(Color.Black, 1,
                        new PointF(random.Next(width), random.Next(height)),
                        new PointF(random.Next(width), random.Next(height))));
                }

                // تبدیل تصویر به فرمت باینری
                using (var ms = new MemoryStream())
                {
                    image.SaveAsPng(ms);
                    var byteArray = ms.ToArray();
                    return File(byteArray, "image/png");
                }
            }
        }

        [HttpPost("RefreshToken")]
        public async Task<ActionResult<RowResultObject<RefreshTokenResultBody>>> RefreshToken(RefreshTokenRequestBody requestBody)
        {
            RowResultObject<RefreshTokenResultBody> result = new RowResultObject<RefreshTokenResultBody>();

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var refreshTokenRecord = await _tokenRep.FindTokenAsync(requestBody.RefreshToken, "RefreshToken");

            if (refreshTokenRecord == null)
            {
                result.ErrorMessage = "رفرش توکن نامعتبر است";
                result.Status = false;
                return BadRequest(result);
            }

            var expireTokenResult = await _tokenRep.MakeTokenExpireAsync(refreshTokenRecord.Result.ID);

            if (expireTokenResult.Status)
            {
                var login = await _loginRep.GetLoginByIdAsync(refreshTokenRecord.Result.UserId, 2);
                var refreshToken = ToolBox.GenerateToken(); // تولید رفرش توکن
                var accessToken = ToolBox.GenerateAccessToken(login.Result); // تولید رفرش توکن
                var refreshTokenExpiryDate = DateTime.Now.ToShamsi().AddDays(30); // تنظیم تاریخ انقضای رفرش توکن برای 30 روز


                var newrefreshTokenRecord = new RefreshToken
                {
                    UserId = login.Result.PersonID,
                    Token = refreshToken, // ذخیره رفرش توکن
                    Type = "RefreshToken", // نوع: RefreshToken
                    Status = true,
                    CreatedDate = DateTime.Now.ToShamsi(),
                    ExpiryDate = refreshTokenExpiryDate // تاریخ انقضا
                };

                var saverefreshToken = await _tokenRep.AddRefreshTokenAsync(newrefreshTokenRecord);

                if (saverefreshToken.Status)
                {
                    result.Status = login.Status;
                    result.ErrorMessage = login.ErrorMessage;
                    result.Result = new RefreshTokenResultBody()
                    {
                        RefreshToken = refreshToken, // بازگرداندن رفرش توکن
                        AccessToken = accessToken, // بازگرداندن اکسس توکن
                    };

                    #region AddLog
                    Log log = new Log()
                    {
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate = DateTime.Now.ToShamsi(),
                        LogTime = DateTime.Now.ToShamsi(),
                        ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
                    };
                    await _logRep.AddLogAsync(log);
                    #endregion

                    return Ok(result);
                }
            }
            else
            {
                result.Status = expireTokenResult.Status;
                result.ErrorMessage = expireTokenResult.ErrorMessage;
            }
            return BadRequest(result);
        }


        [HttpPost("Signup")]
        public async Task<ActionResult<BitResultObject>> Signup(SignupRequestBody signupRequestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(signupRequestBody);
            }

            BitResultObject result = new BitResultObject();
            var validUserName = await _loginRep.ExistLoginAsync(signupRequestBody.UserName,2);

            if (validUserName.Status)
            {
                result.Status = !validUserName.Status;
                result.ErrorMessage = "نام کاربری تکراری است";
                return BadRequest(result);
            }

            var validPhoneNumber = await _loginRep.ExistLoginAsync(signupRequestBody.PhoneNumber, 3);

            if (validPhoneNumber.Status)
            {
                result.Status = !validPhoneNumber.Status;
                result.ErrorMessage = "شماره تماس تکراری است";
                return BadRequest(result);
            }

            var validNaCode = await _loginRep.ExistLoginAsync(signupRequestBody.NaCode, 4);

            if (validNaCode.Status)
            {
                result.Status = !validNaCode.Status;
                result.ErrorMessage = "کد ملی تکراری است";
                return BadRequest(result);
            }

            var validEmail = await _loginRep.ExistLoginAsync(signupRequestBody.Email, 5);

            if (validNaCode.Status)
            {
                result.Status = !validNaCode.Status;
                result.ErrorMessage = "پست الکترونیک تکراری است";
                return BadRequest(result);
            }

            Address address = new Address()
            {
                CityID = signupRequestBody.CityID,
                AddressLocationHorizentalPoint = signupRequestBody.AddressLocationHorizentalPoint,
                AddressLocationVerticalPoint = signupRequestBody.AddressLocationVerticalPoint,
                AddressPostalCode = signupRequestBody.AddressPostalCode,
                AddressStreet = signupRequestBody.AddressStreet,
                Description = signupRequestBody.AddressDescription,
                CreateDate = DateTime.Now.ToShamsi(),
                UpdateDate = DateTime.Now.ToShamsi(),
                
            };

            result = await _addressRep.AddAddressAsync(address);

            if (result.Status)
            {
                Person person = new Person()
                {
                    FirstName = signupRequestBody.FirstName,
                    LastName = signupRequestBody.LastName,
                    NaCode = signupRequestBody.NaCode,
                    Email = signupRequestBody.Email,
                    PhoneNumber = signupRequestBody.PhoneNumber,
                    DateOfBirth = signupRequestBody.DateOfBirth?.StringToDate(),
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    AddressID = address.ID,
                    Description = signupRequestBody.PersonDescription,
                };
                result = await _personRep.AddPersonAsync(person);

                if (result.Status)
                {
                    Customer customer = new Customer()
                    {
                        PersonID = person.ID,
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate= DateTime.Now.ToShamsi(),
                        Description = signupRequestBody.CustomerDescription,
                    };
                    result = await _customerRep.AddCustomerAsync(customer);

                    if (result.Status)
                    {
                        Register register = new Register()
                        {
                            CreateDate = DateTime.Now.ToShamsi(),
                            PersonID = person.ID,
                            RegistrationDate = DateTime.Now.ToShamsi(),
                            UpdateDate = DateTime.Now.ToShamsi(),
                            Description= signupRequestBody.RegisterDescription,
                        };
                        result = await _registerRep.AddRegisterAsync(register);
                        if (result.Status)
                        {
                            Login login = new Login()
                            {
                                Username = signupRequestBody.UserName,
                                PasswordHash = signupRequestBody.Password.ToHash(),
                                PersonID = person.ID,
                                CreateDate = DateTime.Now.ToShamsi(),
                                LastLoginDate = DateTime.Now.ToShamsi(),
                                UpdateDate = DateTime.Now.ToShamsi(),
                                Description = signupRequestBody.LoginDescription,
                            };
                            result = await _loginRep.AddLoginAsync(login);
                            if (result.Status && signupRequestBody.IsStylist)
                            {
                                Stylist stylist = new Stylist()
                                {
                                    JobTypeID = signupRequestBody.JobTypeID,
                                    YearsOfExperience = signupRequestBody.YearsOfExperience,
                                    Specialty = signupRequestBody.Specialty,
                                    StylistParentID = signupRequestBody.StylistParentID,
                                    PersonID = person.ID,
                                    CreateDate = DateTime.Now.ToShamsi(),
                                    UpdateDate = DateTime.Now.ToShamsi(),
                                    Description = login.Description,
                                };
                                result = await _stylistRep.AddStylistAsync(stylist);
                            }
                            if (result.Status)
                            {

                                #region AddLog

                                Log log = new Log()
                                {
                                    CreateDate = DateTime.Now.ToShamsi(),
                                    UpdateDate = DateTime.Now.ToShamsi(),
                                    LogTime = DateTime.Now.ToShamsi(),
                                    ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),

                                };
                                await _logRep.AddLogAsync(log);

                                #endregion

                            }

                            result.ID = person.ID;

                            return Ok(result);
                        }
                    }
                }
            }
            return BadRequest(result);
        }

        [HttpPost("SendSMSCode")]
        public async Task<ActionResult<BitResultObject>> SendSMSCode(SendCodeRequestBody sendCodeRequestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(sendCodeRequestBody);
            }

            BitResultObject result = new BitResultObject();

            var validPhoneNumber = await _loginRep.ExistLoginAsync(sendCodeRequestBody.PhoneNumber, 3);
            if (sendCodeRequestBody.Exists)
            {
                if (!validPhoneNumber.Status && string.IsNullOrEmpty(validPhoneNumber.ErrorMessage))
                {
                    result.Status = validPhoneNumber.Status;
                    result.ErrorMessage = "شماره تماس نامعتبر است";
                    return BadRequest(result);
                }
            }

            else
            {
                if (validPhoneNumber.Status)
                {
                    result.Status = !validPhoneNumber.Status;
                    result.ErrorMessage = "شماره تماس تکراری است";
                    return BadRequest(result);
                }
            }


            result.Status =  await ToolBox.SendCode(sendCodeRequestBody.PhoneNumber);

            if (result.Status)
            {
                result.ErrorMessage = $"کد تایید ارسال شد";
                return Ok(result);
            }
            result.ErrorMessage = $"در ارسال کد مشکلی بوجود آمد";

            return BadRequest(result);
        }

        [HttpPost("CheckSMSCode")]
        public async Task<ActionResult<BitResultObject>> CheckSMSCode(CheckCodeRequestBody checkCodeRequestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(checkCodeRequestBody);
            }

            BitResultObject result = new BitResultObject();

            var validPhoneNumber = await _loginRep.ExistLoginAsync(checkCodeRequestBody.PhoneNumber, 3);
            if (checkCodeRequestBody.Exists)
            {
                if (!validPhoneNumber.Status && string.IsNullOrEmpty(validPhoneNumber.ErrorMessage))
                {
                    result.Status = validPhoneNumber.Status;
                    result.ErrorMessage = "شماره تماس نامعتبر است";
                    return BadRequest(result);
                }
            }

            else
            {
                if (validPhoneNumber.Status)
                {
                    result.Status = !validPhoneNumber.Status;
                    result.ErrorMessage = "شماره تماس تکراری است";
                    return BadRequest(result);
                }
            }


            result.Status = await ToolBox.CheckCode(checkCodeRequestBody.PhoneNumber,checkCodeRequestBody.VerifyCode);

            if (result.Status)
            {
                result.ErrorMessage = $"کد تایید صحیح است";
                return Ok(result);
            }
            else 
            {
                result.ErrorMessage = $"کد تایید صحیح نیست";
                return BadRequest(result);
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<RowResultObject<string>>> ForgotPassword(ForgotPasswordRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            RowResultObject<string> result = new RowResultObject<string>();

            var resetToken = ToolBox.GenerateToken(); // تولید رفرش توکن
            var resetTokenExpiryDate = DateTime.Now.ToShamsi().AddHours(2);

            var existLogin = await _loginRep.ExistLoginAsync(requestBody.Email,5);

            if (existLogin.Status)
            {
                var login = await _loginRep.GetLoginByIdAsync(existLogin.ID,1);
                if (login.Status)
                {

                    var newresetTokenRecord = new RefreshToken
                    {
                        UserId = login.Result.PersonID,
                        Token = resetToken, // ذخیره رفرش توکن
                        Type = "ResetPassword", // نوع: ResetPassword
                        Status = true,
                        CreatedDate = DateTime.Now.ToShamsi(),
                        ExpiryDate = resetTokenExpiryDate // تاریخ انقضا
                    };

                    var saverefreshToken = await _tokenRep.AddRefreshTokenAsync(newresetTokenRecord);

                    if (saverefreshToken.Status)
                    {

                        var fullName = $"{login.Result.Person.FirstName} {login.Result.Person.LastName}";
                        var messageText = ToolBox.MakeResetPasswordMessage(fullName, resetToken);
                        bool sentState = ToolBox.SendEmail(requestBody.Email, "بازنشانی کلمه عبور", messageText);

                        #region AddLog
                        Log log = new Log()
                        {
                            CreateDate = DateTime.Now.ToShamsi(),
                            UpdateDate = DateTime.Now.ToShamsi(),
                            LogTime = DateTime.Now.ToShamsi(),
                            ActionName = this.ControllerContext.RouteData.Values["action"].ToString(),
                        };
                        await _logRep.AddLogAsync(log);
                        #endregion


                        if (sentState)
                        {
                            result.Status = sentState;
                            result.ErrorMessage = $"ایمیلی حاوی لینک بازنشانی رمز عبور برای شما ارسال شد";
                            result.Result = resetToken;
                        }
                        else
                        {
                            result.Status = sentState;
                            result.ErrorMessage = $"در ارسال ایمیلی مشکلی بوجود آمد لطفا دوباره تلاش کنید";
                            result.Result = resetToken;
                        }

                        return Ok(result);
                    }
                    else
                    {
                        result.Status = saverefreshToken.Status;
                        result.ErrorMessage = saverefreshToken.ErrorMessage;
                    }
                }
                else
                {
                    result.Status = login.Status;
                    result.ErrorMessage = login.ErrorMessage;
                }
            }
            else
            {
                result.Status = false;
                result.ErrorMessage = $"پست الکترونیک {requestBody.Email} در سیستم وجود ندارد";
            }
            return BadRequest(result);
        }

        [HttpPost("CheckToken")]
        public async Task<ActionResult<BitResultObject>> CheckToken(CheckTokenRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            BitResultObject result = new BitResultObject();

            var findToken = await _tokenRep.FindTokenAsync(requestBody.Token,requestBody.TokenType,requestBody.TokenStatus);

            result.Status = findToken.Status;
            result.ErrorMessage = findToken.ErrorMessage;

            if (findToken.Status && findToken.Result != null)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("ResetPassword")]
        public async Task<ActionResult<BitResultObject>> ResetPassword(CheckTokenRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            BitResultObject result = new BitResultObject();

            var findToken = await _tokenRep.FindTokenAsync(requestBody.Token, requestBody.TokenType, requestBody.TokenStatus);

            result.Status = findToken.Status;
            result.ErrorMessage = findToken.ErrorMessage;

            if (findToken.Status && findToken.Result != null)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}