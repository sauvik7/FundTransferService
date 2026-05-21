using FundTransfer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FundTransfer.Infrastructure;

public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.AccountId);
            entity.Property(a => a.Balance).IsRequired();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.RequestId).IsRequired();
            entity.Property(t => t.Amount).IsRequired();
            entity.Property(t => t.Status).IsRequired();

            entity.HasIndex(t => t.RequestId)
                .IsUnique(); // 🔥 critical for idempotency safety
        });
    }
}
