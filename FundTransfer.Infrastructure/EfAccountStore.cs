using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FundTransfer.Infrastructure;

public class EfAccountStore(PaymentsDbContext context) : IAccountStore
{
    public async Task<Account?> GetAsync(string accountId)
    {
        return await context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId);
    }

    public async Task AddAsync(Account account)
    {
        await context.Accounts.AddAsync(account);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
