using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;

namespace FundTransfer.Infrastructure;

public class EfAccountStore(PaymentsDbContext context) : IAccountStore
{
    public bool TryGetBalance(string accountId, out decimal balance)
    {
        var account = context.Accounts.Find(accountId);
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
        var account = context.Accounts.Find(accountId);
        if (account is not null)
        {
            return;
        }

        context.Accounts.Add(new Account
        {
            AccountId = accountId,
            Balance = 0m
        });
        context.SaveChanges();
    }

    public void Transfer(string fromAccount, string toAccount, decimal amount, string requestId)
    {
        var sourceAccount = context.Accounts.Find(fromAccount)
            ?? throw new InvalidOperationException("Source account not found");

        var destinationAccount = context.Accounts.Find(toAccount);
        if (destinationAccount is null)
        {
            destinationAccount = new Account
            {
                AccountId = toAccount,
                Balance = 0m
            };
            context.Accounts.Add(destinationAccount);
        }

        sourceAccount.Balance -= amount;
        destinationAccount.Balance += amount;

        context.Transactions.Add(new Transaction
        {
            FromAccount = fromAccount,
            ToAccount = toAccount,
            Amount = amount,
            RequestId = requestId
        });

        context.SaveChanges();
    }
}
