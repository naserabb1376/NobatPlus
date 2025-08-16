using Domain;
using Domains;
using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.Domain;
using NobatPlusDATA.Tools;
using NobatPlusDATA.Views;
using System.Text.RegularExpressions;

namespace NobatPlusDATA.DataLayer
{
    public class NobatPlusContext : DbContext
    {

        public NobatPlusContext(DbContextOptions<NobatPlusContext> options)
      : base(options)
        {
        }

        //Tables

        public DbSet<Person> Persons { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Stylist> Stylists { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<WorkTime> WorkTimes { get; set; }
        public DbSet<SocialNetwork> SocialNetworks { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Register> Registers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SMSMessage> SMSMessages { get; set; }
        public DbSet<CheckAvailability> CheckAvailabilities { get; set; }
        public DbSet<ServiceManagement> ServiceManagements { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<StylistService> StylistServices { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<JobType> JobTypes { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountAssignment> DiscountAssignments { get; set; }
        public DbSet<ServiceDiscount> ServiceDiscounts { get; set; }
        public DbSet<CustomerDiscount> CustomerDiscounts { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ApiGuide> ApiGuides { get; set; }


        // Views

        public DbSet<V_Customer> V_Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // تعریف کلیدهای ترکیبی
            modelBuilder.Entity<BookingService>()
                .HasKey(bs => new { bs.BookingID, bs.ServiceManagementID });

            // تعریف روابط
            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingID);

            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.ServiceManagement)
                .WithMany(sm => sm.BookingServices)
                .HasForeignKey(bs => bs.ServiceManagementID);

            modelBuilder.Entity<StylistService>()
                .HasKey(bs => new { bs.StylistID, bs.ServiceManagementID });

            // تعریف روابط
            modelBuilder.Entity<StylistService>()
                .HasOne(bs => bs.Stylist)
                .WithMany(b => b.StylistServices)
                .HasForeignKey(bs => bs.StylistID);

            modelBuilder.Entity<StylistService>()
                .HasOne(bs => bs.ServiceManagement)
                .WithMany(sm => sm.StylistServices)
                .HasForeignKey(bs => bs.ServiceManagementID);

            // تعریف روابط برای تخفیف‌ها
            modelBuilder.Entity<DiscountAssignment>()
                .HasOne(da => da.Discount)
                .WithMany(d => d.DiscountAssignments)
                .HasForeignKey(da => da.DiscountId);

            modelBuilder.Entity<DiscountAssignment>()
                .HasOne(da => da.Admin)
                .WithMany(a => a.DiscountAssignments)
                .HasForeignKey(da => da.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DiscountAssignment>()
                .HasOne(da => da.Stylist)
                .WithMany(s => s.DiscountAssignments)
                .HasForeignKey(da => da.StylistId)
                .OnDelete(DeleteBehavior.Restrict);

            // تعریف روابط برای تخفیف‌های سرویس‌ها
            modelBuilder.Entity<ServiceDiscount>()
                .HasOne(sd => sd.Discount)
                .WithMany(d => d.ServiceDiscounts)
                .HasForeignKey(sd => sd.DiscountId);

            modelBuilder.Entity<ServiceDiscount>()
                .HasOne(sd => sd.ServiceManagement)
                .WithMany(sm => sm.ServiceDiscounts)
                .HasForeignKey(sd => sd.ServiceManagementId);

            modelBuilder.Entity<ServiceDiscount>()
                .HasOne(sd => sd.Admin)
                .WithMany(a => a.ServiceDiscounts)
                .HasForeignKey(sd => sd.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceDiscount>()
                .HasOne(sd => sd.Stylist)
                .WithMany(s => s.ServiceDiscounts)
                .HasForeignKey(sd => sd.StylistId)
                .OnDelete(DeleteBehavior.Restrict);

            // تعریف روابط برای تخفیف‌های مشتریان
            modelBuilder.Entity<CustomerDiscount>()
                .HasOne(cd => cd.Discount)
                .WithMany(d => d.CustomerDiscounts)
                .HasForeignKey(cd => cd.DiscountId);

            modelBuilder.Entity<CustomerDiscount>()
                .HasOne(cd => cd.Customer)
                .WithMany(c => c.CustomerDiscounts)
                .HasForeignKey(cd => cd.CustomerId);

            modelBuilder.Entity<CustomerDiscount>()
                .HasOne(cd => cd.Stylist)
                .WithMany(s => s.CustomerDiscounts)
                .HasForeignKey(cd => cd.StylistId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(cd => cd.Stylist)
                .WithMany(s => s.Bookings)
                .HasForeignKey(cd => cd.StylistID)
                .OnDelete(DeleteBehavior.NoAction);
            // مدیریت رفتار حذف
            modelBuilder.Entity<BookingService>()
                .HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<StylistService>()
                .HasOne(ss => ss.Stylist)
                .WithMany(s => s.StylistServices)
                .HasForeignKey(ss => ss.StylistID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(cd => cd.Customer)
                .WithMany(s => s.Reviews)
                .HasForeignKey(cd => cd.CustomerID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Address>()
                .HasOne(cd => cd.City)
                .WithMany(s => s.Addresses)
                .HasForeignKey(cd => cd.CityID)
                .OnDelete(DeleteBehavior.NoAction);

            // Map the entity to the view
            modelBuilder.Entity<V_Customer>()
                .HasNoKey()  // Views usually do not have a primary key
                .ToView("V_Customer"); // Name of the view in the database
        }
    }
}