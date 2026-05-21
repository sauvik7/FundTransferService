using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;

namespace FundTransfer.Infrastructure;

public class EfAccountStore : IAccountStore
{
    private readonly PaymentsDbContext _context;

    public EfAccountStore(PaymentsDbContext context)
    {
        _context = context;
    }

    public bool TryGetBalance(string accountId, out decimal balance)
    {
        var account = _context.Accounts.Find(accountId);
        if (account is null)
        {
            balance = 0m;
            return false;
        }

        balance = account.Balance;
        return true;
    }

    public void EnsureAccountExists(string accountId)
    {
        var account = _context.Accounts.Find(accountId);
        if (account is not null)
        {
            return;
        }

        _context.Accounts.Add(new Account
        {
            AccountId = accountId,
            Balance = 0m
        });
        _context.SaveChanges();
    }

    public void Transfer(string fromAccount, string toAccount, decimal amount, string requestId)
    {
        var sourceAccount = _context.Accounts.Find(fromAccount)
            ?? throw new InvalidOperationException("Source account not found");

        var destinationAccount = _context.Accounts.Find(toAccount);
        if (destinationAccount is null)
        {
            destinationAccount = new Account
            {
                AccountId = toAccount,
                Balance = 0m
            };
            _context.Accounts.Add(destinationAccount);
        }

        sourceAccount.Balance -= amount;
        destinationAccount.Balance += amount;

        _context.Transactions.Add(new Transaction
        {
            FromAccount = fromAccount,
            ToAccount = toAccount,
            Amount = amount,
            RequestId = requestId
        });

        _context.SaveChanges();
    }
}
