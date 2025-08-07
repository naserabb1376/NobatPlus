using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using NobatPlusAPI.Tools;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
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

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
    });
            });

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
            builder.Services.AddScoped<INotificationRep, NotificationRep>();
            builder.Services.AddScoped<IPaymentHistoryRep, PaymentHistoryRep>();
            builder.Services.AddScoped<IPaymentRep, PaymentRep>();
            builder.Services.AddScoped<IPersonRep, PersonRep>();
            builder.Services.AddScoped<IRegisterRep, RegisterRep>();
            builder.Services.AddScoped<IReviewRep, ReviewRep>();
            builder.Services.AddScoped<IServiceDiscountRep, ServiceDiscountRep>();
            builder.Services.AddScoped<IServiceManagementRep, ServiceManagementRep>();
            builder.Services.AddScoped<IStylistRep, StylistRep>();
            builder.Services.AddScoped<IStylistServiceRep, StylistServiceRep>();
            builder.Services.AddScoped<ITokenRep, TokenRep>();
            builder.Services.AddScoped<ISocialNetworkRep, SocialNetworkRep>();
            builder.Services.AddScoped<IWorkTimeRep, WorkTimeRep>();
            builder.Services.AddScoped<ISMSMessageRep, SMSMessageRep>();

            #endregion ImportDbServices

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


            var app = builder.Build();

           

            // Configure the HTTP request pipeline.

            #region Pipeline

            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiTitle} {apiVersion}");
                c.RoutePrefix = string.Empty; // روت اصلی سایت برای Swagger
            });

            //}
            app.UseHttpsRedirection();

            app.UseCors(corsPolicy);


            app.UseSession();



            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

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