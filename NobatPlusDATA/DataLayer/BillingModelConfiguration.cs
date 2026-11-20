using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.DataLayer;

internal static class BillingModelConfiguration
{
    internal static void ConfigureBilling(this ModelBuilder model)
    {
        model.Entity<GatewayPaymentRecord>().HasIndex(x => x.TrackingNumber).IsUnique();
        model.Entity<GatewayPaymentRecord>().Property(x => x.Token).HasMaxLength(200);
        model.Entity<GatewayPaymentRecord>().HasIndex(x => x.Token).IsUnique();
        model.Entity<GatewayTransactionRecord>().HasOne<GatewayPaymentRecord>().WithMany()
            .HasForeignKey(x => x.GatewayPaymentRecordID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<SubscriptionPlan>().ToTable("SubscriptionPlans", t =>
        {
            t.HasCheckConstraint("CK_SubscriptionPlans_Values", "[DurationMonths] IN (1,3,6,12) AND [Price] > 0 AND [BookingFeeAmount] >= 0");
            t.HasCheckConstraint("CK_SubscriptionPlans_TypeAndMembers", "[PlanType] IN ('Individual','Business') AND [MaxMemberCount] >= 1");
        });
        model.Entity<SubscriptionPlan>().Property(x => x.Name).HasMaxLength(200);
        model.Entity<SubscriptionPlan>().Property(x => x.PlanType).HasMaxLength(10).HasDefaultValue("Individual");
        model.Entity<SubscriptionPlan>().Property(x => x.MaxMemberCount).HasDefaultValue(1);
        model.Entity<BusinessSubscription>().HasIndex(x => new { x.BusinessID, x.RequestID }).IsUnique();
        model.Entity<BusinessSubscription>().HasIndex(x => x.PaymentID).IsUnique();
        model.Entity<BusinessSubscription>().HasIndex(x => new { x.BusinessID, x.Status, x.StartsAt, x.EndsAt });
        model.Entity<BusinessSubscription>().Property(x => x.PlanTypeSnapshot).HasMaxLength(10).HasDefaultValue("Individual");
        model.Entity<BusinessSubscription>().Property(x => x.MaxMemberCountSnapshot).HasDefaultValue(1);
        model.Entity<BusinessSubscription>().ToTable("BusinessSubscriptions", t =>
            t.HasCheckConstraint("CK_BusinessSubscriptions_Snapshot", "[PlanTypeSnapshot] IN ('Individual','Business') AND [MaxMemberCountSnapshot] >= 1"));
        model.Entity<BusinessSubscription>().HasOne<Worker>().WithMany().HasForeignKey(x => x.BusinessID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BusinessSubscription>().HasOne<SubscriptionPlan>().WithMany().HasForeignKey(x => x.SubscriptionPlanID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BusinessSubscription>().HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingAccount>().HasIndex(x => x.WorkerID).IsUnique();
        model.Entity<BillingAccount>().HasOne<Worker>().WithMany().HasForeignKey(x => x.WorkerID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BusinessBillingSetting>().HasIndex(x => x.BusinessID).IsUnique();
        model.Entity<BusinessBillingSetting>().HasOne<Worker>().WithMany().HasForeignKey(x => x.BusinessID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BusinessBillingSetting>().Property(x => x.SettlementDestination).HasMaxLength(20).HasDefaultValue("Business");
        model.Entity<BusinessBillingSetting>().ToTable("BusinessBillingSettings", t =>
            t.HasCheckConstraint("CK_BusinessBillingSettings_Values", "[DefaultFeePayer] IN ('Business','Worker') AND [CollectionMode] IN ('Prepaid','Postpaid','Hybrid') AND [InvoiceCycle] IN ('Weekly','Monthly') AND [SettlementDestination] IN ('Business','Separate') AND [CreditLimit] >= 0 AND [DueDays] >= 0"));
        model.Entity<WorkerBillingSetting>().HasIndex(x => new { x.BusinessID, x.WorkerID }).IsUnique();
        model.Entity<WorkerBillingSetting>().Property(x => x.RevenueSharePercent).HasDefaultValue(0m);
        model.Entity<WorkerBillingSetting>().ToTable("WorkerBillingSettings", t =>
            t.HasCheckConstraint("CK_WorkerBillingSettings_RevenueSharePercent", "[RevenueSharePercent] >= 0 AND [RevenueSharePercent] <= 100"));
        model.Entity<WorkerBillingSetting>().HasOne<Worker>().WithMany().HasForeignKey(x => x.BusinessID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<WorkerBillingSetting>().HasOne<Worker>().WithMany().HasForeignKey(x => x.WorkerID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<WorkerBillingSetting>().HasOne<Person>().WithMany().HasForeignKey(x => x.AcceptedByPersonID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BookingPlatformFee>().HasIndex(x => x.BookingID).IsUnique();
        model.Entity<BookingPlatformFee>().HasIndex(x => new { x.BillingAccountID, x.AccruedAt });
        model.Entity<BookingPlatformFee>().HasOne<Booking>().WithMany().HasForeignKey(x => x.BookingID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BookingPlatformFee>().HasOne<Worker>().WithMany().HasForeignKey(x => x.BusinessID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BookingPlatformFee>().HasOne<Worker>().WithMany().HasForeignKey(x => x.WorkerID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BookingPlatformFee>().HasOne<BillingAccount>().WithMany().HasForeignKey(x => x.BillingAccountID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BookingPlatformFee>().HasOne<BusinessSubscription>().WithMany().HasForeignKey(x => x.BusinessSubscriptionID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BookingPlatformFee>().ToTable("BookingPlatformFees", t =>
            t.HasCheckConstraint("CK_BookingPlatformFees_Values", "[FeeAmount] >= 0 AND [Status] IN ('accrued','reversed')"));
        model.Entity<BillingTransaction>().HasOne<BillingAccount>().WithMany().HasForeignKey(x => x.BillingAccountID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingTransaction>().Property(x => x.SourceType).HasMaxLength(40);
        model.Entity<BillingTransaction>().Property(x => x.TransactionType).HasMaxLength(30);
        model.Entity<BillingTransaction>().HasIndex(x => new { x.BillingAccountID, x.TransactionType, x.SourceType, x.SourceID }).IsUnique();
        model.Entity<BillingTransaction>().ToTable("BillingTransactions", t =>
            t.HasCheckConstraint("CK_BillingTransactions_Amount", "[Amount] <> 0"));
        model.Entity<BillingInvoice>().HasOne<BillingAccount>().WithMany().HasForeignKey(x => x.BillingAccountID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingInvoice>().HasIndex(x => new { x.BillingAccountID, x.PeriodStart, x.PeriodEnd }).IsUnique();
        model.Entity<BillingInvoice>().ToTable("BillingInvoices", t =>
            t.HasCheckConstraint("CK_BillingInvoices_Period", "[PeriodEnd] > [PeriodStart] AND [TotalAmount] > 0"));
        model.Entity<BillingInvoiceItem>().HasOne<BillingInvoice>().WithMany().HasForeignKey(x => x.BillingInvoiceID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingInvoiceItem>().HasOne<BookingPlatformFee>().WithMany().HasForeignKey(x => x.BookingPlatformFeeID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingInvoiceItem>().HasIndex(x => x.BookingPlatformFeeID).IsUnique();
        model.Entity<BillingPayment>().HasOne<BillingAccount>().WithMany().HasForeignKey(x => x.BillingAccountID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingPayment>().HasOne<Payment>().WithMany().HasForeignKey(x => x.PaymentID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingPayment>().HasOne<BillingInvoice>().WithMany().HasForeignKey(x => x.BillingInvoiceID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingPayment>().HasIndex(x => x.PaymentID).IsUnique();
        model.Entity<BillingPayment>().HasIndex(x => new { x.BillingAccountID, x.RequestID }).IsUnique();
        model.Entity<BillingPayment>().ToTable("BillingPayments", t =>
            t.HasCheckConstraint("CK_BillingPayments_Amount", "[Amount] > 0"));
        model.Entity<BillingAllocation>().HasOne<BillingPayment>().WithMany().HasForeignKey(x => x.BillingPaymentID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingAllocation>().HasOne<BookingPlatformFee>().WithMany().HasForeignKey(x => x.BookingPlatformFeeID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingAllocation>().HasIndex(x => new { x.BillingPaymentID, x.BookingPlatformFeeID }).IsUnique().HasFilter("[ReversedAt] IS NULL");
        model.Entity<BillingAllocation>().ToTable("BillingAllocations", t =>
            t.HasCheckConstraint("CK_BillingAllocations_Amount", "[Amount] > 0"));
        model.Entity<BillingRefund>().HasOne<BillingPayment>().WithMany().HasForeignKey(x => x.BillingPaymentID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingRefund>().HasOne<Person>().WithMany().HasForeignKey(x => x.RequestedByPersonID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingRefund>().HasOne<Person>().WithMany().HasForeignKey(x => x.ResolvedByPersonID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingRefund>().HasIndex(x => new { x.BillingPaymentID, x.RequestID }).IsUnique();
        model.Entity<BillingRefund>().Property(x => x.BankReference).HasMaxLength(200);
        model.Entity<BillingRefund>().HasIndex(x => x.BankReference).IsUnique().HasFilter("[BankReference] IS NOT NULL AND [Status] = N'confirmed'");
        model.Entity<BillingRefund>().ToTable("BillingRefunds", t =>
            t.HasCheckConstraint("CK_BillingRefunds_Values", "[Amount] > 0 AND [Status] IN ('pending','confirmed','rejected')"));
        model.Entity<BillingSettingHistory>().HasOne<Worker>().WithMany().HasForeignKey(x => x.BusinessID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingSettingHistory>().HasOne<Worker>().WithMany().HasForeignKey(x => x.WorkerID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<BillingSettingHistory>().HasOne<Person>().WithMany().HasForeignKey(x => x.ActorPersonID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<Payment>().Property(x => x.Purpose).HasMaxLength(30).HasDefaultValue(PaymentPurposes.Service);
        model.Entity<Payment>().Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IRR");
        model.Entity<Payment>().HasOne<Person>().WithMany().HasForeignKey(x => x.OwnerPersonID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<PaymentHistory>().Property(x => x.GatewayName).HasMaxLength(60);
        model.Entity<PaymentHistory>().Property(x => x.TransactionCode).HasMaxLength(200);
        model.Entity<PaymentHistory>().HasIndex(x => new { x.GatewayName, x.TransactionCode }).IsUnique()
            .HasFilter("[PaymentStatus] = 1 AND [GatewayRequestState] = N'verified' AND [GatewayName] IS NOT NULL AND [TransactionCode] IS NOT NULL");
    }
}
