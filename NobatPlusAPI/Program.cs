using AITechDATA.DataLayer.Services;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MTPermissionCenter.Abstractions;
using MTPermissionCenter.AspNetCore;
using MTPermissionCenter.EFCore;
using Newtonsoft.Json;
using NobatPlusAPI.DataLayer.Services;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using NobatPlusDATA.Tools;
using NobatPlusTokenDB.DataLayer;
using Parbad.Builder;
using Parbad.Gateway.ParbadVirtual;
using Parbad.Gateway.ZarinPal;
using Repositories;
using Services;
using System.Globalization;
using System.Text;

namespace NobatPlusAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var corsPolicy = builder.Configuration["cors:policy"].ToString();
            var cookiesecurity = builder.Configuration["cors:cookiesecurity"].ToString();

            var allowedOrigins = builder.Configuration.GetSection("cors:allowedOrigins").Get<List<string>>().ToArray();
            var payGatewaySettings = builder.Configuration.GetSection("PaymentGateways").Get<List<PayGatewaySettings>>().ToList();
            var zarinPalGatewaySetting = payGatewaySettings.FirstOrDefault(x => x.GatewayName.ToLower() == "zarinpal");

            var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddMemoryCache();
            builder.Services.AddHttpContextAccessor();
            if (cookiesecurity == "default")
            {
                builder.Services.AddSession();
            }
            else
            {
                builder.Services.AddSession(options =>
                {
                    options.Cookie.HttpOnly = true; // ????? ????? ???? ???????
                    options.Cookie.IsEssential = true; // ????? ???? ???? ???? ?????? Session
                    options.Cookie.SameSite = SameSiteMode.None;  // ????? ????? ??????? ?? ??????????? cross-origin
                    options.Cookie.SecurePolicy = (CookieSecurePolicy)int.Parse(cookiesecurity);  // ??? HTTPS ???? ???
                });
            }


            builder.Services.AddCors(options =>
            {

                if (corsPolicy.ToLower().Contains("allowall"))
                {
                    options.AddPolicy(corsPolicy, builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .WithExposedHeaders("Set-Cookie");

                    });
                }
                else
                {
                    options.AddPolicy(corsPolicy, builder =>
                    builder.WithOrigins(allowedOrigins) // اضافه کردن localhost و آی‌پی لوکال
                           .AllowCredentials()
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                             .WithExposedHeaders("Set-Cookie"));

                }
            });

            //if (corsSettings.useRateLimiter)
            //{
            //    ///--محدود کردن نرخ درخواست‌ها (Rate Limiting) --///
            //    builder.Services.AddRateLimiter(options =>
            //    {
            //        options.AddFixedWindowLimiter("Fixed", limiterOptions =>
            //        {
            //            limiterOptions.Window = TimeSpan.FromSeconds(10);
            //            limiterOptions.PermitLimit = 5; // تعداد درخواست‌ها در هر بازه زمانی
            //        });
            //    });
            //}


            // Add services to the container.

            var apiVersion = ToolBox.CalculateAppVersionNo();
            var apiTitle = builder.Environment.ApplicationName;

            builder.Services.AddControllers(options =>
            {
                //options.OutputFormatters.Add()
                options.ReturnHttpNotAcceptable = true;
                options.Filters.AddService<AdminAuditLogActionFilter>();
            })
              .AddNewtonsoftJson(options =>
              {
                  options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
              })
             .AddXmlDataContractSerializerFormatters();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = apiTitle,
                    Version = apiVersion
                });

                // Configure Swagger to use JWT authentication
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Please enter your JWT with Bearer into the field",

                    //Reference = new OpenApiReference
                    //{
                    //    Id = JwtBearerDefaults.AuthenticationScheme,
                    //    Type = ReferenceType.SecurityScheme
                    //}
                };

                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);


                c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = new List<string>()
                });

            });

            #region AddDbContext

            var mainDbconfigHelper = new MainDbConfigurationHelper();

            builder.Services.AddDbContext<NobatPlusContext>(options =>
                options.UseSqlServer(mainDbconfigHelper.GetConnectionString("publicdb")));


            var tokenDbconfigHelper = new TokenDbConfigurationHelper();

            builder.Services.AddDbContext<RefreshTokenDBContext>(options =>
                options.UseSqlServer(tokenDbconfigHelper.GetConnectionString("Tokenpublicdb")));

            #endregion

            builder.Services.AddMTPermissionCenter();

            #region ImportDbServices

            builder.Services.AddScoped<IAddressRep, AddressRep>();
            builder.Services.AddScoped<IAdminRep, AdminRep>();
            builder.Services.AddScoped<IAdminActionCenterRep, AdminActionCenterRep>();
            builder.Services.AddScoped<IAdminAuditLogRep, AdminAuditLogRep>();
            builder.Services.AddScoped<IAdminDashboardRep, AdminDashboardRep>();
            builder.Services.AddScoped<IAdminMonitoringRep, AdminMonitoringRep>();
            builder.Services.AddScoped<IBookingRep, BookingRep>();
            builder.Services.AddScoped<IBookingServiceRep, BookingServiceRep>();
            builder.Services.AddScoped<IBookingServiceOptionValueRep, BookingServiceOptionValueRep>();
            builder.Services.AddScoped<ICheckAvailabilityRep, CheckAvailabilityRep>();
            builder.Services.AddScoped<ICityRep, CityRep>();
            builder.Services.AddScoped<ICustomerDiscountRep, CustomerDiscountRep>();
            builder.Services.AddScoped<ICustomerRep, CustomerRep>();
            builder.Services.AddScoped<ICustomerDashboardRep, CustomerDashboardRep>();
            builder.Services.AddScoped<IDiscountAssignmentRep, DiscountAssignmentRep>();
            builder.Services.AddScoped<IDiscountRep, DiscountRep>();
            builder.Services.AddScoped<IJobTypeRep, JobTypeRep>();
            builder.Services.AddScoped<IApiGuideRep, ApiGuideRep>();
            builder.Services.AddScoped<ILoginRep, LoginRep>();
            builder.Services.AddScoped<ILogRep, LogRep>();
            builder.Services.AddScoped<IImageRep, ImageRep>();
            builder.Services.AddScoped<IFileUploadRep, FileUploadRep>();
            builder.Services.AddScoped<IFinancialAccountRep, FinancialAccountRep>();
            builder.Services.AddScoped<INotificationRep, NotificationRep>();
            builder.Services.AddScoped<IPaymentHistoryRep, PaymentHistoryRep>();
            builder.Services.AddScoped<IPaymentBookingRep, PaymentBookingRep>();
            builder.Services.AddScoped<IPaymentDetailOptionValueRep, PaymentDetailOptionValueRep>();
            builder.Services.AddScoped<IPaymentRep, PaymentRep>();
            builder.Services.AddScoped<IWalletRep, WalletRep>();
            builder.Services.AddScoped<IPersonRep, PersonRep>();
            builder.Services.AddScoped<IRegisterRep, RegisterRep>();
            builder.Services.AddScoped<IReviewRep, ReviewRep>();
            builder.Services.AddScoped<IRoleRep, RoleRep>();
            builder.Services.AddScoped<IServiceDiscountRep, ServiceDiscountRep>();
            builder.Services.AddScoped<IServiceManagementRep, ServiceManagementRep>();
            builder.Services.AddScoped<IServiceOptionRep, ServiceOptionRep>();
            builder.Services.AddScoped<IServiceOptionValueRep, ServiceOptionValueRep>();
            builder.Services.AddScoped<IStylistRep, StylistRep>();
            builder.Services.AddScoped<IStylistDashboardRep, StylistDashboardRep>();
            builder.Services.AddScoped<IStylistServiceRep, StylistServiceRep>();
            builder.Services.AddScoped<IStylistServicePriceVariantRep, StylistServicePriceVariantRep>();
            builder.Services.AddScoped<IStylistServicePriceVariantOptionValueRep, StylistServicePriceVariantOptionValueRep>();
            builder.Services.AddScoped<IStylistPacificRep, StylistPacificRep>();
            builder.Services.AddScoped<ISupportTicketRep, SupportTicketRep>();
            builder.Services.AddScoped<ITokenRep, TokenRep>();
            builder.Services.AddScoped<ISocialNetworkRep, SocialNetworkRep>();
            builder.Services.AddScoped<IWorkTimeRep, WorkTimeRep>();
            builder.Services.AddScoped<ISMSMessageRep, SMSMessageRep>();
            builder.Services.AddScoped<IRateQuestionRep, RateQuestionRep>();
            builder.Services.AddScoped<IRateHistoryRep, RateHistoryRep>();
            builder.Services.AddScoped<ISettingRep, SettingRep>();
            builder.Services.AddScoped<IUserRoleProvider, PersonRep>();
            builder.Services.AddScoped<IPermissionRep, PermissionRep>();
            builder.Services.AddScoped<IPermissionRoleRep, PermissionRoleRep>();
            builder.Services.AddScoped<IUserPermissionRep, UserPermissionRep>();
            builder.Services.AddScoped<AdminAuditLogActionFilter>();

            #endregion ImportDbServices

            #region PayGatewayConfigs

            builder.Services.AddParbad()
                .ConfigureGateways(gateways =>
                {
                    gateways
                        .AddZarinPal()
                        .WithAccounts(accounts =>
                        {
                            accounts.AddInMemory(account =>
                            {
                                account.MerchantId = zarinPalGatewaySetting?.MerchantId ?? "";
                                account.IsSandbox = zarinPalGatewaySetting?.IsSandbox ?? true;
                            });
                        });
                    gateways
                .AddParbadVirtual()
                .WithOptions(options => options.GatewayPath = "/MyVirtualGateway");
                })
                .ConfigureStorage(storage => storage.UseMemoryCache());

            #endregion

            #region ImportMTPermissionCenterServices

            builder.Services.AddScoped<IPermissionInvalidationService, PermissionInvalidationService>();
            builder.Services.AddScoped<IPermissionService, EfPermissionService<NobatPlusContext>>();

            #endregion ImportMTPermissionCenterServices

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
  .AddJwtBearer(options =>
  {
      options.RequireHttpsMetadata = false;
      options.SaveToken = true;
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = builder.Configuration["Jwt:Issuer"],
          ValidAudience = builder.Configuration["Jwt:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(key)
      };
  });

            // 1) اسکن فقط اسمبلی برنامه (پیشنهادی)
            builder.Services.AddAutoMapper(cfg => { /* تنظیمات سراسری اختیاری */ },
                                           typeof(Program).Assembly);

            builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage((mainDbconfigHelper.GetConnectionString("publicdb"))));
            builder.Services.AddHangfireServer();


            builder.Services.AddTransient<JobManager>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.

            #region Pipeline

            app.UseStaticFiles();   

            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiTitle} {apiVersion}");
                c.RoutePrefix = string.Empty; // روت اصلی سایت برای Swagger
                c.InjectJavascript("/js/swagger-token.js");
            });

            //}
            app.UseHttpsRedirection();

            app.UseParbadVirtualGateway();

            app.UseCors(corsPolicy);


            app.UseSession();

            #region HangFire

            app.UseHangfireDashboard("/hangfire");
            //app.UseHangfireServer();

            // 👇 اینجا دقیقاً محل ثبت Job هست
            RecurringJob.AddOrUpdate<JobManager>(
     job => job.ProcessTodayBirthdays(),
     "0 9 * * *",
     GetIranTimeZone()
 );

            #endregion

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // IMPORTANT: after authentication
            app.UseMTPermissionCenter();

            //Controller/Action/Id?
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseParbadVirtualGateway();


            #endregion Pipeline

            app.Run();
        }

        private static TimeZoneInfo GetIranTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
            }
            catch (InvalidTimeZoneException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
            }
        }
    }
}
