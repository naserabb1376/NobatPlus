using Domain;
using Domains;
using Microsoft.EntityFrameworkCore;
using MTPermissionCenter.EFCore;
using MTPermissionCenter.EFCore.Entities;
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

        //public NobatPlusContext()
        //{

        //}

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
        public DbSet<PaymentDetail> PaymentDetails { get; set; }
        public DbSet<PaymentBooking> PaymentBookings { get; set; }
        public DbSet<Login> Logins { get; set; }
        public DbSet<Register> Registers { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<FinancialAccount> FinancialAccounts { get; set; }
        public DbSet<FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<SettlementRequest> SettlementRequests { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SMSMessage> SMSMessages { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportTicketMessage> SupportTicketMessages { get; set; }
        public DbSet<CheckAvailability> CheckAvailabilities { get; set; }
        public DbSet<ServiceManagement> ServiceManagements { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<BookingServiceOptionValue> BookingServiceOptionValues { get; set; }
        public DbSet<StylistService> StylistServices { get; set; }
        public DbSet<ServiceOption> ServiceOptions { get; set; }
        public DbSet<ServiceOptionValue> ServiceOptionValues { get; set; }
        public DbSet<StylistServicePriceVariant> StylistServicePriceVariants { get; set; }
        public DbSet<StylistServicePriceVariantOptionValue> StylistServicePriceVariantOptionValues { get; set; }
        public DbSet<PaymentDetailOptionValue> PaymentDetailOptionValues { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<JobType> JobTypes { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountAssignment> DiscountAssignments { get; set; }
        public DbSet<ServiceDiscount> ServiceDiscounts { get; set; }
        public DbSet<CustomerDiscount> CustomerDiscounts { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
        public DbSet<FileUpload> FileUploads { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ApiGuide> ApiGuides { get; set; }
        public DbSet<RateQuestion> RateQuestions { get; set; }
        public DbSet<RateHistory> RateHistories { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<StylistPacific> StylistPacifics { get; set; }
        public DbSet<Setting> Settings { get; set; }


        // Views

        public DbSet<V_Customer> V_Customers { get; set; }


        // MTPermissionCenter

        public DbSet<MTPermissionCenter_Permission> Permissions { get; set; }
        public DbSet<MTPermissionCenter_PermissionRole> PermissionRoles { get; set; }
        public DbSet<MTPermissionCenter_UserPermission> UserPermissions { get; set; }



        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    MainDbConfigurationHelper configurationHelper = new MainDbConfigurationHelper();
        //    optionsBuilder.UseSqlServer(configurationHelper.GetConnectionString("publicdb"));
        //    //  base.OnConfiguring(optionsBuilder);
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // dynamic auth config
            modelBuilder.AddMTPermissionCenter();

            foreach (var property in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(entityType => entityType.GetProperties())
                         .Where(property => property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // demo config
            modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<Stylist>()
                .Property(x => x.SlotIntervalMinutes)
                .HasDefaultValue(30);

            modelBuilder.Entity<Stylist>()
                .Property(x => x.BookingCreationMode)
                .HasDefaultValue("automatic");

            modelBuilder.Entity<Stylist>()
                .Property(x => x.SlotDisplayMode)
                .HasDefaultValue("all");

            modelBuilder.Entity<AdminAuditLog>()
                .HasOne(x => x.ActorPerson)
                .WithMany()
                .HasForeignKey(x => x.ActorPersonID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AdminAuditLog>()
                .HasIndex(x => x.OccurredAt);

            modelBuilder.Entity<AdminAuditLog>()
                .HasIndex(x => new { x.ActorPersonID, x.OccurredAt });

            modelBuilder.Entity<AdminAuditLog>()
                .HasIndex(x => new { x.EntityName, x.ActionName });

            modelBuilder.Entity<FileUpload>()
                .HasOne(x => x.ReviewedByPerson)
                .WithMany()
                .HasForeignKey(x => x.ReviewedByPersonID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FileUpload>()
                .HasIndex(x => new { x.ReviewStatus, x.CreateDate });

            modelBuilder.Entity<SupportTicket>()
                .HasOne(x => x.Person)
                .WithMany()
                .HasForeignKey(x => x.PersonID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SupportTicket>()
                .HasOne(x => x.AssignedAdminPerson)
                .WithMany()
                .HasForeignKey(x => x.AssignedAdminPersonID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SupportTicket>()
                .HasIndex(x => new { x.Status, x.Priority, x.LastMessageAt });

            modelBuilder.Entity<SupportTicket>()
                .HasIndex(x => new { x.PersonID, x.LastMessageAt });

            modelBuilder.Entity<SupportTicketMessage>()
                .HasOne(x => x.SupportTicket)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.SupportTicketID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportTicketMessage>()
                .HasOne(x => x.SenderPerson)
                .WithMany()
                .HasForeignKey(x => x.SenderPersonID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SupportTicketMessage>()
                .HasIndex(x => new { x.SupportTicketID, x.CreateDate });



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

            modelBuilder.Entity<BookingServiceOptionValue>()
                .HasKey(x => new { x.BookingID, x.ServiceManagementID, x.ServiceOptionValueID });

            modelBuilder.Entity<BookingServiceOptionValue>()
                .HasOne(x => x.BookingService)
                .WithMany(x => x.OptionValues)
                .HasForeignKey(x => new { x.BookingID, x.ServiceManagementID })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookingServiceOptionValue>()
                .HasOne(x => x.ServiceOptionValue)
                .WithMany()
                .HasForeignKey(x => x.ServiceOptionValueID)
                .OnDelete(DeleteBehavior.NoAction);

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

            modelBuilder.Entity<ServiceOption>()
                .HasOne(x => x.ServiceManagement)
                .WithMany()
                .HasForeignKey(x => x.ServiceManagementID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceOption>()
                .HasIndex(x => new { x.ServiceManagementID, x.OptionKey })
                .IsUnique();

            modelBuilder.Entity<ServiceOptionValue>()
                .HasOne(x => x.ServiceOption)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.ServiceOptionID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StylistServicePriceVariant>()
                .HasOne(x => x.StylistService)
                .WithMany(x => x.PriceVariants)
                .HasForeignKey(x => new { x.StylistID, x.ServiceManagementID })
                .HasPrincipalKey(x => new { x.StylistID, x.ServiceManagementID })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StylistServicePriceVariant>()
                .HasIndex(x => new { x.StylistID, x.ServiceManagementID, x.OptionValueCombinationKey })
                .IsUnique();

            modelBuilder.Entity<StylistServicePriceVariantOptionValue>()
                .HasKey(x => new { x.StylistServicePriceVariantID, x.ServiceOptionValueID });

            modelBuilder.Entity<StylistServicePriceVariantOptionValue>()
                .HasOne(x => x.StylistServicePriceVariant)
                .WithMany(x => x.OptionValues)
                .HasForeignKey(x => x.StylistServicePriceVariantID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StylistServicePriceVariantOptionValue>()
                .HasOne(x => x.ServiceOptionValue)
                .WithMany()
                .HasForeignKey(x => x.ServiceOptionValueID)
                .OnDelete(DeleteBehavior.NoAction);

            // تعریف روابط برای تخفیف‌ها
            modelBuilder.Entity<DiscountAssignment>()
                .HasOne(da => da.Discount)
                .WithMany(d => d.DiscountAssignments)
                .HasForeignKey(da => da.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasForeignKey(sd => sd.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasForeignKey(cd => cd.DiscountId)
                 .OnDelete(DeleteBehavior.Cascade);

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
            modelBuilder.Entity<Booking>()
                .HasIndex(x => new { x.StylistID, x.BookingDate, x.IsCancelled });
            modelBuilder.Entity<Booking>()
                .HasIndex(x => new { x.CustomerID, x.BookingDate, x.IsCancelled });
            modelBuilder.Entity<CheckAvailability>()
                .HasIndex(x => new { x.StylistID, x.Date, x.Time });
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

            modelBuilder.Entity<PaymentBooking>()
                .HasKey(pb => new { pb.PaymentID, pb.BookingID });

            modelBuilder.Entity<PaymentBooking>()
                .HasOne(pb => pb.Booking)
                .WithMany(b => b.PaymentBookings)
                .HasForeignKey(pb => pb.BookingID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentBooking>()
                .HasOne(pb => pb.Payment)
                .WithMany(p => p.PaymentBookings)
                .HasForeignKey(pb => pb.PaymentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                .Ignore(p => p.Discount);

            modelBuilder.Entity<PaymentDetail>()
                     .HasOne(ss => ss.Payment)
                     .WithMany(s => s.PaymentDetails)
                     .HasForeignKey(ss => ss.PaymentID)
                     .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.Booking)
                .WithMany()
                .HasForeignKey(pd => pd.BookingID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.Stylist)
                .WithMany(s => s.PaymentDetails)
                .HasForeignKey(pd => pd.StylistID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.ServiceManagement)
                .WithMany(s => s.PaymentDetails)
                .HasForeignKey(pd => pd.ServiceManagementID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.StylistService)
                .WithMany()
                .HasForeignKey(pd => new { pd.StylistID, pd.ServiceManagementID })
                .HasPrincipalKey(ss => new { ss.StylistID, ss.ServiceManagementID })
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentDetail>()
                .HasOne(pd => pd.StylistServicePriceVariant)
                .WithMany()
                .HasForeignKey(pd => pd.StylistServicePriceVariantID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentDetailOptionValue>()
                .HasKey(x => new { x.PaymentDetailID, x.ServiceOptionValueID });

            modelBuilder.Entity<PaymentDetailOptionValue>()
                .HasOne(x => x.PaymentDetail)
                .WithMany(x => x.OptionValues)
                .HasForeignKey(x => x.PaymentDetailID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentDetailOptionValue>()
                .HasOne(x => x.ServiceOptionValue)
                .WithMany()
                .HasForeignKey(x => x.ServiceOptionValueID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PaymentHistory>()
                .HasOne(ph => ph.Payment)
                .WithMany()
                .HasForeignKey(ph => ph.PaymentID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.Customer)
                .WithOne(c => c.Wallet)
                .HasForeignKey<Wallet>(w => w.CustomerID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.CustomerID)
                .IsUnique();

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(wt => wt.WalletID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Booking)
                .WithMany()
                .HasForeignKey(wt => wt.BookingID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Payment)
                .WithMany()
                .HasForeignKey(wt => wt.PaymentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FinancialAccount>()
                .HasOne(fa => fa.Stylist)
                .WithOne(s => s.FinancialAccount)
                .HasForeignKey<FinancialAccount>(fa => fa.StylistID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FinancialAccount>()
                .HasIndex(fa => fa.StylistID)
                .IsUnique();

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(ft => ft.FinancialAccount)
                .WithMany(fa => fa.Transactions)
                .HasForeignKey(ft => ft.FinancialAccountID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(ft => ft.Booking)
                .WithMany()
                .HasForeignKey(ft => ft.BookingID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(ft => ft.Payment)
                .WithMany()
                .HasForeignKey(ft => ft.PaymentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(ft => ft.SettlementRequest)
                .WithMany(sr => sr.Transactions)
                .HasForeignKey(ft => ft.SettlementRequestID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SettlementRequest>()
                .HasOne(sr => sr.FinancialAccount)
                .WithMany(fa => fa.SettlementRequests)
                .HasForeignKey(sr => sr.FinancialAccountID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RateHistory>()
     .HasOne(r => r.Customer)
     .WithMany(c => c.RateHistories)
     .HasForeignKey(r => r.CustomerID)
     .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RateHistory>()
                .HasOne(r => r.Stylist)
                .WithMany(s => s.RateHistories)
                .HasForeignKey(r => r.StylistID)
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
