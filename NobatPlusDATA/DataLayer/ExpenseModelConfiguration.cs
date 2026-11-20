using Microsoft.EntityFrameworkCore;
using NobatPlusDATA.Domain;

namespace NobatPlusDATA.DataLayer;

internal static class ExpenseModelConfiguration
{
    internal static void ConfigureExpenses(this ModelBuilder model)
    {
        model.Entity<ExpenseCategory>().Property(x => x.Name).HasMaxLength(150).IsRequired();
        model.Entity<ExpenseCategory>().HasIndex(x => new { x.OwnerWorkerID, x.Name }).IsUnique();
        model.Entity<ExpenseCategory>().HasIndex(x => x.Name).IsUnique()
            .HasFilter("[OwnerWorkerID] IS NULL");
        model.Entity<ExpenseCategory>().HasOne(x => x.OwnerWorker).WithMany()
            .HasForeignKey(x => x.OwnerWorkerID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<ExpenseCategory>().HasOne(x => x.CreatedByPerson).WithMany()
            .HasForeignKey(x => x.CreatedByPersonID).OnDelete(DeleteBehavior.NoAction);

        model.Entity<Expense>().Property(x => x.Title).HasMaxLength(200).IsRequired();
        model.Entity<Expense>().Property(x => x.AmountIRR).IsRequired();
        model.Entity<Expense>().Property(x => x.ExpenseDate).HasColumnType("date");
        model.Entity<Expense>().ToTable("Expenses", t =>
            t.HasCheckConstraint("CK_Expenses_AmountIRR", "[AmountIRR] > 0"));
        model.Entity<Expense>().HasOne(x => x.OwnerWorker).WithMany()
            .HasForeignKey(x => x.OwnerWorkerID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<Expense>().HasOne(x => x.ExpenseCategory).WithMany(x => x.Expenses)
            .HasForeignKey(x => x.ExpenseCategoryID).OnDelete(DeleteBehavior.NoAction);
        model.Entity<Expense>().HasOne(x => x.CreatedByPerson).WithMany()
            .HasForeignKey(x => x.CreatedByPersonID).OnDelete(DeleteBehavior.NoAction);
    }
}
