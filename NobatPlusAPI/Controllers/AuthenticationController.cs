using Domain;
using Domains;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
using System.Net;

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
        private const string AccessTokenCookieName = "nobatix_access_token";
        private const string RefreshTokenCookieName = "nobatix_refresh_token";

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

        private CookieOptions GetRefreshTokenCookieOptions(DateTime expires)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expires,
                IsEssential = true,
                Path = "/authentication"
            };
        }

        private CookieOptions GetAccessTokenCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddHours(1),
                IsEssential = true,
                Path = "/"
            };
        }

        private string? GetRefreshTokenFromRequest(RefreshTokenRequestBody? requestBody)
        {
            if (!string.IsNullOrWhiteSpace(requestBody?.RefreshToken))
            {
                return requestBody.RefreshToken;
            }

            return Request.Cookies.TryGetValue(RefreshTokenCookieName, out var cookieRefreshToken)
                ? cookieRefreshToken
                : null;
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expires)
        {
            Response.Cookies.Append(RefreshTokenCookieName, refreshToken, GetRefreshTokenCookieOptions(expires));
        }

        private void SetAccessTokenCookie(string accessToken)
        {
            Response.Cookies.Append(AccessTokenCookieName, accessToken, GetAccessTokenCookieOptions());
        }

        private void ClearAccessTokenCookie()
        {
            Response.Cookies.Delete(AccessTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
        }

        private void ClearRefreshTokenCookie()
        {
            Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/authentication"
            });
        }

        private void ClearAuthCookies()
        {
            ClearAccessTokenCookie();
            ClearRefreshTokenCookie();
        }

        [HttpPost("Authenticate")]
        public async Task<ActionResult<RowResultObject<AuthenticationResultBody>>> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            RowResultObject<AuthenticationResultBody> result = new RowResultObject<AuthenticationResultBody>();
            RowResultObject<Login> authenticateResult = new RowResultObject<Login>();

            try
            {
#if DEBUG
                if (authenticationRequestBody.LoginType <= 0)
                {
                   // authenticationRequestBody.UserName = "09136857124";
                    authenticationRequestBody.Password = "569022mt";
                    authenticationRequestBody.LoginType = 1;
                }
#endif

                var storedCaptchaCode = HttpContext.Session.GetString("CaptchaCode");

                if (!authenticationRequestBody.CaptchaCode.ValidateCaptcha(storedCaptchaCode))
                {
                    result.Status = false;
                    result.ErrorMessage = "کد کپچا نادرست است.";
                    return BadRequest(result);
                }

                result = await DoLoginAsync(authenticationRequestBody);

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message}\n{ex.InnerException?.Message}";
            }

           if (result.Status)
            {
                return Ok(result);
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

        [HttpPost("CheckCaptchaCode")]
        public async Task<ActionResult<BitResultObject>> CheckCaptchaCode(CheckCaptchaCodeRequestBody checkCodeRequestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(checkCodeRequestBody);
            }

            BitResultObject result = new BitResultObject();

            try
            {

                var storedCaptchaCode = HttpContext.Session.GetString("CaptchaCode");
                if (checkCodeRequestBody.CaptchaCode.ValidateCaptcha(storedCaptchaCode))
                {
                    result.Status = true;
                    result.ErrorMessage = "";
                    return Ok(result);
                }

                else
                {
                    result.Status = false;
                    result.ErrorMessage = "کد کپچا نادرست است.";
                    return BadRequest(result);
                }

            }
            catch (Exception ex)
            {
                result.Status = false;
                result.ErrorMessage = $"{ex.Message}\n{ex.InnerException?.Message}";
            }


            return BadRequest(result);
        }


        [HttpPost("RefreshToken")]
        public async Task<ActionResult<RowResultObject<RefreshTokenResultBody>>> RefreshToken(RefreshTokenRequestBody? requestBody)
        {
            RowResultObject<RefreshTokenResultBody> result = new RowResultObject<RefreshTokenResultBody>();

            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }

            var currentRefreshToken = GetRefreshTokenFromRequest(requestBody);
            if (string.IsNullOrWhiteSpace(currentRefreshToken))
            {
                result.ErrorMessage = "رفرش توکن نامعتبر است";
                result.Status = false;
                ClearAuthCookies();
                return BadRequest(result);
            }

            var refreshTokenRecord = await _tokenRep.FindTokenAsync(currentRefreshToken, "RefreshToken");

            if (!refreshTokenRecord.Status || refreshTokenRecord.Result == null)
            {
                result.ErrorMessage = "رفرش توکن نامعتبر است";
                result.Status = false;
                ClearAuthCookies();
                return BadRequest(result);
            }

            var expireTokenResult = await _tokenRep.MakeTokenExpireAsync(refreshTokenRecord.Result.ID);

            if (expireTokenResult.Status)
            {
                var login = await _loginRep.GetLoginByIdAsync(refreshTokenRecord.Result.UserId, 2);
                if (!login.Status || login.Result == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "اطلاعات کاربر یافت نشد";
                    ClearAuthCookies();
                    return BadRequest(result);
                }

                var profileContext = await ResolveProfileContextAsync(
                    login.Result.PersonID,
                    login.Result.Person.RoleId,
                    requestBody?.ActiveProfileType ?? refreshTokenRecord.Result.ActiveProfileType,
                    requestBody?.ActiveProfileId > 0
                        ? requestBody.ActiveProfileId
                        : refreshTokenRecord.Result.ActiveProfileId ?? 0);
                if (!profileContext.Status)
                {
                    result.Status = false;
                    result.ErrorMessage = profileContext.ErrorMessage;
                    return BadRequest(result);
                }

                var refreshToken = ToolBox.GenerateToken(); // تولید رفرش توکن
                var accessToken = GenerateProfileAccessToken(login.Result, profileContext);
                var refreshTokenExpiryDate = DateTime.Now.ToShamsi().AddDays(30); // تنظیم تاریخ انقضای رفرش توکن برای 30 روز


                var newrefreshTokenRecord = new RefreshToken
                {
                    UserId = login.Result.PersonID,
                    Token = refreshToken, // ذخیره رفرش توکن
                    Type = "RefreshToken", // نوع: RefreshToken
                    Status = true,
                    CreatedDate = DateTime.Now.ToShamsi(),
                    ExpiryDate = refreshTokenExpiryDate, // تاریخ انقضا
                    ActiveProfileId = profileContext.ActiveProfileId > 0 ? profileContext.ActiveProfileId : null,
                    ActiveProfileType = string.IsNullOrWhiteSpace(profileContext.ActiveProfileType)
                        ? null
                        : profileContext.ActiveProfileType
                };

                var saverefreshToken = await _tokenRep.AddRefreshTokenAsync(newrefreshTokenRecord);

                if (saverefreshToken.Status)
                {
                    SetAccessTokenCookie(accessToken);
                    SetRefreshTokenCookie(refreshToken, refreshTokenExpiryDate);
                    result.Status = login.Status;
                    result.ErrorMessage = login.ErrorMessage;
                    result.Result = new RefreshTokenResultBody()
                    {
                        RefreshToken = refreshToken, // بازگرداندن رفرش توکن
                        AccessToken = accessToken, // بازگرداندن اکسس توکن
                        RoleId = profileContext.RoleId,
                        StylistId = profileContext.StylistId,
                        SalonId = profileContext.SalonId,
                        ActiveProfileId = profileContext.ActiveProfileId,
                        ActiveProfileType = profileContext.ActiveProfileType,
                        Profiles = profileContext.Profiles,
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

        [Authorize]
        [HttpPost("SwitchProfile")]
        public async Task<ActionResult<RowResultObject<AuthenticationResultBody>>> SwitchProfile(
            SwitchProfileRequestBody requestBody)
        {
            var result = new RowResultObject<AuthenticationResultBody>();
            if (!ModelState.IsValid)
                return BadRequest(requestBody);

            var personId = User.GetCurrentUserId();
            var login = await _loginRep.GetLoginByIdAsync(personId, 2);
            if (!login.Status || login.Result?.Person == null)
            {
                result.Status = false;
                result.ErrorMessage = "اطلاعات کاربر یافت نشد";
                return BadRequest(result);
            }

            var profileContext = await ResolveProfileContextAsync(
                personId,
                login.Result.Person.RoleId,
                requestBody.ProfileType,
                requestBody.ProfileId);
            if (!profileContext.Status || profileContext.ActiveProfileId <= 0)
            {
                result.Status = false;
                result.ErrorMessage = profileContext.ErrorMessage;
                return BadRequest(result);
            }

            var customer = await _customerRep.ExistCustomerAsync(personId.ToString(), "personid");
            var accessToken = GenerateProfileAccessToken(login.Result, profileContext);
            var currentRefreshToken = GetRefreshTokenFromRequest(null);
            if (!string.IsNullOrWhiteSpace(currentRefreshToken))
            {
                var currentTokenRecord = await _tokenRep.FindTokenAsync(currentRefreshToken, "RefreshToken");
                if (currentTokenRecord.Status && currentTokenRecord.Result != null)
                    await _tokenRep.MakeTokenExpireAsync(currentTokenRecord.Result.ID);
            }

            var refreshToken = ToolBox.GenerateToken();
            var refreshTokenExpiryDate = DateTime.Now.ToShamsi().AddDays(30);
            var saveRefreshToken = await _tokenRep.AddRefreshTokenAsync(new RefreshToken
            {
                UserId = personId,
                Token = refreshToken,
                Type = "RefreshToken",
                Status = true,
                CreatedDate = DateTime.Now.ToShamsi(),
                ExpiryDate = refreshTokenExpiryDate,
                ActiveProfileId = profileContext.ActiveProfileId,
                ActiveProfileType = profileContext.ActiveProfileType
            });
            if (!saveRefreshToken.Status)
            {
                result.Status = false;
                result.ErrorMessage = saveRefreshToken.ErrorMessage;
                return BadRequest(result);
            }

            SetAccessTokenCookie(accessToken);
            SetRefreshTokenCookie(refreshToken, refreshTokenExpiryDate);

            result.Result = new AuthenticationResultBody
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                PersonId = personId,
                CustomerId = customer.ID,
                StylistId = profileContext.StylistId,
                SalonId = profileContext.SalonId,
                ActiveProfileId = profileContext.ActiveProfileId,
                ActiveProfileType = profileContext.ActiveProfileType,
                Profiles = profileContext.Profiles,
                RoleId = profileContext.RoleId,
                FirstName = login.Result.Person.FirstName,
                LastName = login.Result.Person.LastName,
                IsActive = login.Result.Person.IsActive
            };

            return Ok(result);
        }


        [HttpPost("Signup")]
        public async Task<ActionResult<BitResultObject>> Signup(SignupRequestBody signupRequestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(signupRequestBody);
            }

            BitResultObject result = new BitResultObject();

            Address address = new Address();

            var validUserName = await _loginRep.ExistLoginAsync(signupRequestBody.PhoneNumber, "PhoneNumber");

            if (validUserName.Status)
            {
                result.Status = !validUserName.Status;
                result.ErrorMessage = "نام کاربری (شماره موبایل) تکراری است";
                return BadRequest(result);
            }

            var validPhoneNumber = await _loginRep.ExistLoginAsync(signupRequestBody.PhoneNumber, "PhoneNumber");

            if (validPhoneNumber.Status)
            {
                result.Status = !validPhoneNumber.Status;
                result.ErrorMessage = "شماره موبایل تکراری است";
                return BadRequest(result);
            }

            var validNaCode =  (await _loginRep.ExistLoginAsync(signupRequestBody.NaCode, "NationalCode"));

            if ((!string.IsNullOrEmpty(signupRequestBody.NaCode.Trim())) && validNaCode.Status)
            {
                result.Status = !validNaCode.Status;
                result.ErrorMessage = "کد ملی تکراری است";
                return BadRequest(result);
            }

            //var validEmail = await _loginRep.ExistLoginAsync(signupRequestBody.Email, "Email");

            //if (validEmail.Status)
            //{
            //    result.Status = !validNaCode.Status;
            //    result.ErrorMessage = "پست الکترونیک تکراری است";
            //    return BadRequest(result);
            //}


            if (signupRequestBody.Address != null)
            {
                address = new Address()
                {
                    CityID = signupRequestBody.Address.CityID,
                    AddressLocationHorizentalPoint = signupRequestBody.Address.AddressLocationHorizentalPoint,
                    AddressLocationVerticalPoint = signupRequestBody.Address.AddressLocationVerticalPoint,
                    Description = signupRequestBody.Address.AddressDescription,
                    AddressPostalCode = signupRequestBody.Address.AddressPostalCode ?? "",
                    AddressStreet = signupRequestBody.Address.AddressStreet,
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                };

                result = await _addressRep.AddAddressAsync(address);
            }

            if (result.Status)
            {
                Person person = new Person()
                {
                    FirstName = signupRequestBody.FirstName,
                    LastName = signupRequestBody.LastName,
                    NaCode = signupRequestBody.NaCode ?? "",
                    Email = signupRequestBody.Email ?? "",
                    PhoneNumber = signupRequestBody.PhoneNumber,
                    Gender = signupRequestBody.Gender,
                    RoleId = GetRoleId(signupRequestBody),
                    DateOfBirth = signupRequestBody.DateOfBirth.StringToDate(),
                    CreateDate = DateTime.Now.ToShamsi(),
                    UpdateDate = DateTime.Now.ToShamsi(),
                    AddressID = (address != null && address.ID > 0) ? address.ID : null,
                    IsActive = signupRequestBody.IsActive,
                    Description = "",
                };
                result = await _personRep.AddPersonAsync(person);

                if (result.Status)
                {
                    Customer customer = new Customer()
                    {
                        PersonID = person.ID,
                        CreateDate = DateTime.Now.ToShamsi(),
                        UpdateDate= DateTime.Now.ToShamsi(),
                        Description = "",
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
                            Description= "",
                        };
                        result = await _registerRep.AddRegisterAsync(register);
                        if (result.Status)
                        {
                          
                            if (result.Status && signupRequestBody.stylist != null)
                            {
                                Stylist stylist = new Stylist()
                                {
                                    JobTypeID = signupRequestBody.stylist.JobTypeID,
                                    YearsOfExperience = signupRequestBody.stylist.YearsOfExperience,
                                    Specialty = signupRequestBody.stylist.Specialty ?? "",
                                    StylistParentID = signupRequestBody.stylist.StylistParentID,
                                    PersonID = person.ID,
                                    CreateDate = DateTime.Now.ToShamsi(),
                                    UpdateDate = DateTime.Now.ToShamsi(),
                                    Description = signupRequestBody.stylist.Description ?? "",
                                    AccountStatus = signupRequestBody.stylist.AccountStatus ?? "",
                                    GenderAccepted = signupRequestBody.stylist.GenderAccepted ?? "",
                                    IsWorkShop = signupRequestBody.stylist.IsWorkshop,
                                    PayMethod = signupRequestBody.stylist.PayMethod ?? "",
                                    StylistBio = signupRequestBody.stylist.StylistBio ?? "",
                                    StylistName = signupRequestBody.stylist.StylistName ?? "",
                                    WorkShopDepositAmount = signupRequestBody.stylist.WorkShopRentAmount,
                                    WorkShopInteractMode = signupRequestBody.stylist.WorkShopInteractMode ?? "",
                                    WorkShopRentAmount = signupRequestBody.stylist.WorkShopRentAmount,
                                    RestTime = signupRequestBody.stylist.RestTime,
                                    SlotIntervalMinutes = signupRequestBody.stylist.SlotIntervalMinutes <= 0 ? 30 : signupRequestBody.stylist.SlotIntervalMinutes,
                                    BookingCreationMode = signupRequestBody.stylist.BookingCreationMode ?? "automatic",
                                    SlotDisplayMode = signupRequestBody.stylist.SlotDisplayMode ?? "all",
                                    IsActive = false,
                                    IsStylistOnly= signupRequestBody.stylist.IsStylistOnly,

                                };
                                result = await _stylistRep.AddStylistAsync(stylist);
                            }
                            if (result.Status)
                            {

                                var theLogin = new Login()
                                {
                                    
                                    CreateDate = DateTime.Now.ToShamsi(),
                                    UpdateDate = DateTime.Now.ToShamsi(),
                                    LastLoginDate = DateTime.Now.ToShamsi(),
                                    Description = "",
                                    PasswordHash = "",
                                    PersonID = person.ID,
                                    Username = signupRequestBody.PhoneNumber,
                                };

                                result = await _loginRep.AddLoginAsync(theLogin);
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


                                    result.ID = person.ID;

                                    if (result.Status && signupRequestBody.WithLogin)
                                    {
                                        AuthenticationRequestBody authenticationRequestBody = new AuthenticationRequestBody()
                                        {
                                            CaptchaCode = "",
                                            Password="",
                                            UserName = signupRequestBody.PhoneNumber,
                                            LoginType = 4
                                        };
                                        var loginResult = await DoLoginAsync(authenticationRequestBody);

                                        if (!loginResult.Status)
                                            return BadRequest(loginResult);

                                        return Ok(loginResult);
                                    }

                                    return Ok(result);

                                }


                            }

                        }
                    }
                }
            }
            return BadRequest(result);
        }

        private int GetRoleId(SignupRequestBody signupRequestBody)
        {
            int roleId = 0;

            if (signupRequestBody.stylist == null)
            {
                roleId = 1;
            }
            else if (signupRequestBody.stylist.IsWorkshop)
            {
                roleId = 3;
            }
            else
            {
                roleId = 2;
            }

            return roleId;
        }

        [HttpPost("SendSMSCode")]
        public async Task<ActionResult<BitResultObject>> SendSMSCode(SendCodeRequestBody sendCodeRequestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(sendCodeRequestBody);
            }

            BitResultObject result = new BitResultObject();

            var validPhoneNumber = await _loginRep.ExistLoginAsync(sendCodeRequestBody.PhoneNumber, "PhoneNumber");
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


            var sendCodeResult =  await ToolBox.SendCode(sendCodeRequestBody.PhoneNumber);
            result.Status =  sendCodeResult.SendStatus;

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

            var validPhoneNumber = await _loginRep.ExistLoginAsync(checkCodeRequestBody.PhoneNumber, "PhoneNumber");
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

            if (string.IsNullOrEmpty(requestBody.PhoneNumber) && string.IsNullOrEmpty(requestBody.Email))
            {
                result.Status = false;
                result.ErrorMessage = $"ورود حداقل یکی از مقادیر خواسته شده الزامی است";
            }

            if (!string.IsNullOrEmpty(requestBody.PhoneNumber))
            {
                var validPhoneNumber = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");
                if (!validPhoneNumber.Status && string.IsNullOrEmpty(validPhoneNumber.ErrorMessage))
                {
                    result.Status = validPhoneNumber.Status;
                    result.ErrorMessage = "شماره تماس نامعتبر است";
                    return BadRequest(result);
                }

                var sendCodeResult = await ToolBox.SendCode(requestBody.PhoneNumber);
                result.Status = sendCodeResult.SendStatus;

                if (result.Status)
                {
                    result.ErrorMessage = $"کد تایید ارسال شد";
                    return Ok(result);
                }
            }
            else if (!string.IsNullOrEmpty(requestBody.Email))
            {
                var resetTokenExpiryDate = DateTime.Now.ToShamsi().AddHours(2);

                var existLogin = await _loginRep.ExistLoginAsync(requestBody.Email, "Email");

                if (existLogin.Status)
                {
                    var login = await _loginRep.GetLoginByIdAsync(existLogin.ID, 1);
                    var resetToken = ToolBox.GenerateToken(login.Result.ID); // تولید رفرش توکن

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
            }

            return BadRequest(result);
        }


        [HttpPost("ResetPassword")]
        public async Task<ActionResult<BitResultObject>> ResetPassword(ResetPasswordRequestBody requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            BitResultObject result = new BitResultObject();

            if (string.IsNullOrEmpty(requestBody.VerifyCode) && string.IsNullOrEmpty(requestBody.Token))
            {
                result.Status = false;
                result.ErrorMessage = $"ورود حداقل یکی از مقادیر توکن یا کد تایید الزامی است";
            }

            if (!string.IsNullOrEmpty(requestBody.VerifyCode))
            {
                var validPhoneNumber = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");
                if (!validPhoneNumber.Status && string.IsNullOrEmpty(validPhoneNumber.ErrorMessage))
                {
                    result.Status = validPhoneNumber.Status;
                    result.ErrorMessage = "شماره تماس نامعتبر است";
                    return BadRequest(result);
                }

                result.Status = await ToolBox.CheckCode(requestBody.PhoneNumber, requestBody.VerifyCode);

                if (result.Status)
                {

                    var existLogin = await _loginRep.ExistLoginAsync(requestBody.PhoneNumber, "PhoneNumber");

                    if (existLogin.Status)
                    {
                        var login = await _loginRep.GetLoginByIdAsync(existLogin.ID, 1);

                        if (login.Status)
                        {
                            login.Result.PasswordHash = requestBody.NewPassword.ToHash();

                            var saveLogin = await _loginRep.EditLoginAsync(login.Result);

                            if (saveLogin.Status)
                            {
                                result.Status = saveLogin.Status;
                                result.ErrorMessage = $"تغییر کلمه عبور با موفقیت انجام شد";

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
                            result.Status = login.Status;
                            result.ErrorMessage = login.ErrorMessage;
                            return BadRequest(result);
                        }

                    }
                    else
                    {
                        result.Status = existLogin.Status;
                        result.ErrorMessage = "شماره موبایل وارد شده در سیستم وجود ندارد";
                        return BadRequest(result);
                    }
                }
                else
                {
                    result.ErrorMessage = $"کد تایید صحیح نیست";
                    return BadRequest(result);
                }
            }
            else if (!string.IsNullOrEmpty(requestBody.Token))
            {
                long loginId = long.Parse(requestBody.Token.Split('-')[0]);

                var existLogin = await _loginRep.ExistLoginAsync(loginId.ToString(), "loginId");

                if (loginId > 0 && existLogin.Status)
                {
                    var login = await _loginRep.GetLoginByIdAsync(loginId, 1);

                    if (login.Status)
                    {
                        login.Result.PasswordHash = requestBody.NewPassword.ToHash();

                        var saveLogin = await _loginRep.EditLoginAsync(login.Result);

                        if (saveLogin.Status)
                        {
                            result.Status = saveLogin.Status;
                            result.ErrorMessage = $"تغییر کلمه عبور با موفقیت انجام شد";

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
                            result.Status = saveLogin.Status;
                            result.ErrorMessage = saveLogin.ErrorMessage;
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
                    result.ErrorMessage = "کاربر معتبر نیست";
                }
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

        [HttpPost("LogOut")]
        public async Task<ActionResult<BitResultObject>> LogOut(RefreshTokenRequestBody? requestBody)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(requestBody);
            }
            BitResultObject result = new BitResultObject() { Status = true,ErrorMessage =""};

            var currentRefreshToken = GetRefreshTokenFromRequest(requestBody);
            if (string.IsNullOrWhiteSpace(currentRefreshToken))
            {
                ClearAuthCookies();
                result.ErrorMessage = $"کاربر از سیستم خارج شد";
                return Ok(result);
            }

            var refreshTokenRecord = await _tokenRep.FindTokenAsync(currentRefreshToken, "RefreshToken");

            if (refreshTokenRecord.Status && refreshTokenRecord.Result != null)
            {
                var expireTokenResult = await _tokenRep.MakeTokenExpireAsync(refreshTokenRecord.Result.ID);

                if (expireTokenResult.Status)
                {
                    result.Status = expireTokenResult.Status;
                    result.ErrorMessage = $"کاربر از سیستم خارج شد";
                    ClearAuthCookies();

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
                else
                {
                    result.Status = expireTokenResult.Status;
                    result.ErrorMessage = expireTokenResult.ErrorMessage;
                    return BadRequest(result);
                }
            }

            ClearAuthCookies();
            return Ok(result);
        }

        private async Task<ProfileContextResult> ResolveProfileContextAsync(
            long personId,
            long personRoleId,
            string? requestedProfileType,
            long requestedProfileId)
        {
            var result = new ProfileContextResult
            {
                RoleId = personRoleId
            };
            var profilesResult = await _stylistRep.GetStylistProfilesByPersonIdAsync(personId);
            if (!profilesResult.Status)
            {
                result.Status = false;
                result.ErrorMessage = profilesResult.ErrorMessage;
                return result;
            }

            var profiles = profilesResult.Results ?? new List<StylistProfileDTO>();
            result.Profiles = profiles.Select(x => new AuthenticationProfileBody
            {
                ID = x.ID,
                ProfileType = x.ProfileType,
                Name = x.Name,
                ParentId = x.StylistParentID,
                IsActive = x.IsActive,
                AccountStatus = x.AccountStatus
            }).ToList();
            result.StylistId = profiles.FirstOrDefault(x => x.ProfileType == "stylist")?.ID ?? 0;
            result.SalonId = profiles.FirstOrDefault(x => x.ProfileType == "salon")?.ID ?? 0;

            var normalizedType = requestedProfileType?.Trim().ToLowerInvariant() ?? "";
            var hasRequestedProfile = requestedProfileId > 0 || !string.IsNullOrWhiteSpace(normalizedType);
            StylistProfileDTO? activeProfile;
            if (hasRequestedProfile)
            {
                activeProfile = profiles.FirstOrDefault(x =>
                    (requestedProfileId <= 0 || x.ID == requestedProfileId) &&
                    (string.IsNullOrWhiteSpace(normalizedType) || x.ProfileType == normalizedType));
                if (activeProfile == null)
                {
                    result.Status = false;
                    result.ErrorMessage = "پروفایل انتخاب‌شده متعلق به این کاربر نیست";
                    return result;
                }
            }
            else
            {
                var defaultType = personRoleId switch
                {
                    (long)DbTools.BaseRole.Salon => "salon",
                    (long)DbTools.BaseRole.Stylist => "stylist",
                    _ => ""
                };
                activeProfile = profiles.FirstOrDefault(x =>
                    x.IsActive && x.ProfileType == defaultType);
            }

            if (activeProfile != null)
            {
                if (!activeProfile.IsActive)
                {
                    result.Status = false;
                    result.ErrorMessage = "پروفایل انتخاب‌شده غیرفعال است";
                    return result;
                }

                result.ActiveProfileId = activeProfile.ID;
                result.ActiveProfileType = activeProfile.ProfileType;
                result.RoleId = activeProfile.ProfileType == "salon"
                    ? (long)DbTools.BaseRole.Salon
                    : (long)DbTools.BaseRole.Stylist;
            }

            return result;
        }

        private static string GenerateProfileAccessToken(Login login, ProfileContextResult profile)
        {
            return ToolBox.GenerateAccessToken(
                login,
                profile.ActiveProfileId,
                profile.ActiveProfileType,
                profile.StylistId,
                profile.SalonId,
                profile.RoleId);
        }

        private sealed class ProfileContextResult
        {
            public bool Status { get; set; } = true;
            public string ErrorMessage { get; set; } = "";
            public long RoleId { get; set; }
            public long StylistId { get; set; }
            public long SalonId { get; set; }
            public long ActiveProfileId { get; set; }
            public string ActiveProfileType { get; set; } = "";
            public List<AuthenticationProfileBody> Profiles { get; set; } = new();
        }

        private async Task<RowResultObject<AuthenticationResultBody>> DoLoginAsync(AuthenticationRequestBody requestBody)
        {
            RowResultObject<AuthenticationResultBody> result = new RowResultObject<AuthenticationResultBody>();
            RowResultObject<Login> authenticateResult = new RowResultObject<Login>();

            try
            {
                switch (requestBody.LoginType)
                {
                    default:
                    case 1:
                        authenticateResult = await _loginRep.AuthenticateAsync(requestBody.UserName, requestBody.Password, requestBody.LoginType);
                        break;
                    case 2:
                        var validPhoneNumber = await _loginRep.ExistLoginAsync(requestBody.UserName, "UserName");
                        if (!validPhoneNumber.Status && string.IsNullOrEmpty(validPhoneNumber.ErrorMessage))
                        {
                            authenticateResult.Status = validPhoneNumber.Status;
                            authenticateResult.ErrorMessage = "شماره تماس نامعتبر است";
                            //return BadRequest(result);
                        }
                        bool validCode = await ToolBox.CheckCode(requestBody.UserName, requestBody.Password);
                        if (validCode)
                        {
                            authenticateResult = await _loginRep.AuthenticateAsync(requestBody.UserName, requestBody.Password, requestBody.LoginType);
                        }
                        else
                        {
                            authenticateResult.Status = validCode;
                            authenticateResult.ErrorMessage = "کد تایید نامعتبر است";
                            //return BadRequest(result);
                        }
                        break;
                    case 3:
                        authenticateResult = await _loginRep.AuthenticateAsync(requestBody.UserName, requestBody.Password, requestBody.LoginType);
                        break;
                    case 4:
                        authenticateResult = await _loginRep.AuthenticateAsync(requestBody.UserName, requestBody.Password, requestBody.LoginType);
                        break;
                }

                result.Status = authenticateResult.Status;
                result.ErrorMessage = authenticateResult.ErrorMessage;

                if (authenticateResult.Status)
                {
                    var profileContext = await ResolveProfileContextAsync(
                        authenticateResult.Result.PersonID,
                        authenticateResult.Result.Person.RoleId,
                        null,
                        0);
                    if (!profileContext.Status)
                    {
                        result.Status = false;
                        result.ErrorMessage = profileContext.ErrorMessage;
                        return result;
                    }

                    var refreshToken = ToolBox.GenerateToken(); // تولید رفرش توکن
                    var accessToken = GenerateProfileAccessToken(authenticateResult.Result, profileContext);
                    var refreshTokenExpiryDate = DateTime.Now.ToShamsi().AddDays(30); // تنظیم تاریخ انقضای رفرش توکن برای 30 روز


                    var refreshTokenRecord = new RefreshToken
                    {
                        UserId = authenticateResult.Result.PersonID,
                        Token = refreshToken, // ذخیره رفرش توکن
                        Type = "RefreshToken", // نوع: RefreshToken
                        Status = true,
                        CreatedDate = DateTime.Now.ToShamsi(),
                        ExpiryDate = refreshTokenExpiryDate, // تاریخ انقضا
                        ActiveProfileId = profileContext.ActiveProfileId > 0 ? profileContext.ActiveProfileId : null,
                        ActiveProfileType = string.IsNullOrWhiteSpace(profileContext.ActiveProfileType)
                            ? null
                            : profileContext.ActiveProfileType
                    };

                    var saverefreshToken = await _tokenRep.AddRefreshTokenAsync(refreshTokenRecord);
                    var iscustomer = await _customerRep.ExistCustomerAsync(authenticateResult.Result.PersonID.ToString(), "personid");

                    if (saverefreshToken.Status)
                    {
                        SetAccessTokenCookie(accessToken);
                        SetRefreshTokenCookie(refreshToken, refreshTokenExpiryDate);
                        result.Status = authenticateResult.Status;
                        result.ErrorMessage = authenticateResult.ErrorMessage;
                        result.Result = new AuthenticationResultBody()
                        {
                            RefreshToken = refreshToken, // بازگرداندن رفرش توکن
                            AccessToken = accessToken, // بازگرداندن اکسس توکن
                            PersonId = authenticateResult.Result.PersonID,
                            CustomerId = iscustomer.ID,
                            StylistId = profileContext.StylistId,
                            SalonId = profileContext.SalonId,
                            ActiveProfileId = profileContext.ActiveProfileId,
                            ActiveProfileType = profileContext.ActiveProfileType,
                            Profiles = profileContext.Profiles,
                            IsActive = authenticateResult.Result.Person.IsActive,
                            RoleId = profileContext.RoleId,
                            FirstName = authenticateResult.Result.Person.FirstName,
                            LastName = authenticateResult.Result.Person.LastName,
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

                        //return Ok(result);
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

            return result;
        }
    }
}
