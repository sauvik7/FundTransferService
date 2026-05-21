using FundTransfer.Domain.Entities;
using FundTransfer.Domain.Services;
using FundTransfer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FundTransfer.Tests;

public class EfAccountStoreTests
{
    [Fact]
    public async Task Transfer_PersistsAccountBalances()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(databaseName: "EfAccountStoreTests")
            .Options;

        using var context = new PaymentsDbContext(options);

        var store = new EfAccountStore(context);

        // ✅ Arrange: create accounts using domain constructor
        var acc1 = new Account("ACC1", 1000m);
        var acc2 = new Account("ACC2", 0m);

        await store.AddAsync(acc1);
        await store.AddAsync(acc2);
        await store.SaveChangesAsync();

        // ✅ Act: perform transfer via domain service
        var from = await store.GetAsync("ACC1");
        var to = await store.GetAsync("ACC2");

        var domainService = new TransferDomainService();
        domainService.Execute(from!, to!, "req-test-transfer", 250m);

        await store.SaveChangesAsync();

        // ✅ Assert
        var updatedFrom = await store.GetAsync("ACC1");
        var updatedTo = await store.GetAsync("ACC2");

        Assert.Equal(750m, updatedFrom!.Balance);
        Assert.Equal(250m, updatedTo!.Balance);
    }
}
