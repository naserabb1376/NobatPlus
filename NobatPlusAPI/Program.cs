using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.DataLayer.Services;
using System.Globalization;
using System.Text;

namespace NobatPlusAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });


            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                //options.OutputFormatters.Add()
                options.ReturnHttpNotAcceptable = true;
            }).AddNewtonsoftJson()
             .AddXmlDataContractSerializerFormatters();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                //  c.SwaggerDoc("v1", new OpenApiInfo { Title = "OneApi", Version = "v1" });

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
            });  
            
            //}
            app.UseHttpsRedirection();

            app.UseCors("AllowAll");


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