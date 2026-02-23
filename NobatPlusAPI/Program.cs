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

            var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

            builder.Services.AddDistributedMemoryCache();
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

          //  builder.Services.AddMTPermissionCenter();

            #region ImportDbServices

            builder.Services.AddScoped<IAddressRep, AddressRep>();
            builder.Services.AddScoped<IAdminRep, AdminRep>();
            builder.Services.AddScoped<IBookingRep, BookingRep>();
            builder.Services.AddScoped<IBookingServiceRep, BookingServiceRep>();
            builder.Services.AddScoped<ICheckAvailabilityRep, CheckAvailabilityRep>();
            builder.Services.AddScoped<ICityRep, CityRep>();
            builder.Services.AddScoped<ICustomerDiscountRep, CustomerDiscountRep>();
            builder.Services.AddScoped<ICustomerRep, CustomerRep>();
            builder.Services.AddScoped<IDiscountAssignmentRep, DiscountAssignmentRep>();
            builder.Services.AddScoped<IDiscountRep, DiscountRep>();
            builder.Services.AddScoped<IJobTypeRep, JobTypeRep>();
            builder.Services.AddScoped<IApiGuideRep, ApiGuideRep>();
            builder.Services.AddScoped<ILoginRep, LoginRep>();
            builder.Services.AddScoped<ILogRep, LogRep>();
            builder.Services.AddScoped<IImageRep, ImageRep>();
            builder.Services.AddScoped<IFileUploadRep, FileUploadRep>();
            builder.Services.AddScoped<INotificationRep, NotificationRep>();
            builder.Services.AddScoped<IPaymentHistoryRep, PaymentHistoryRep>();
            builder.Services.AddScoped<IPaymentRep, PaymentRep>();
            builder.Services.AddScoped<IPersonRep, PersonRep>();
            builder.Services.AddScoped<IRegisterRep, RegisterRep>();
            builder.Services.AddScoped<IReviewRep, ReviewRep>();
            builder.Services.AddScoped<IRoleRep, RoleRep>();
            builder.Services.AddScoped<IServiceDiscountRep, ServiceDiscountRep>();
            builder.Services.AddScoped<IServiceManagementRep, ServiceManagementRep>();
            builder.Services.AddScoped<IStylistRep, StylistRep>();
            builder.Services.AddScoped<IStylistServiceRep, StylistServiceRep>();
            builder.Services.AddScoped<IStylistPacificRep, StylistPacificRep>();
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

            #endregion ImportDbServices


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

            app.UseCors(corsPolicy);


            app.UseSession();

            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            // IMPORTANT: after authentication
            //app.UseMTPermissionCenter();

            //Controller/Action/Id?
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #endregion Pipeline

            app.Run();
        }
    }
}