using FundTransfer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FundTransfer.Tests;

public class EfAccountStoreTests
{
    [Fact]
    public void Transfer_PersistsAccountBalancesAndTransaction()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName: "EfAccountStoreTests")
            .Options;

        using var context = new PaymentsDbContext(options);
        context.Database.EnsureCreated();

        var store = new EfAccountStore(context);
        store.EnsureAccountExists("ACC1");
        store.EnsureAccountExists("ACC2");

        context.Accounts.Single(a => a.AccountId == "ACC1").Balance = 1000m;
        context.SaveChanges();

        store.Transfer("ACC1", "ACC2", 250m, "req-test-transfer");

        var source = context.Accounts.Single(a => a.AccountId == "ACC1");
        var destination = context.Accounts.Single(a => a.AccountId == "ACC2");

        Assert.Equal(750m, source.Balance);
        Assert.Equal(250m, destination.Balance);
        Assert.Single(context.Transactions.Where(t => t.FromAccount == "ACC1" && t.ToAccount == "ACC2"));
    }
}
