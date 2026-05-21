using FundTransfer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FundTransfer.Infrastructure;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountId)
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(e => e.Balance)
                .HasPrecision(18, 2)
                .IsRequired();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FromAccount)
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(e => e.ToAccount)
                .HasMaxLength(32)
                .IsRequired();
            entity.Property(e => e.Amount)
                .HasPrecision(18, 2)
                .IsRequired();
            entity.Property(e => e.RequestId)
                .HasMaxLength(64)
                .IsRequired();
            entity.Property(e => e.CreatedAt)
                .IsRequired();
        });
    }
}
