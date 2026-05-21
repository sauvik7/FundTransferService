namespace FundTransfer.Domain.Entities;

#nullable enable

public class Account
{
    public string AccountId { get; private set; } = null!;
    public decimal Balance { get; private set; }

    public Account(string accountId, decimal balance)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("AccountId is required");

        if (balance < 0)
            throw new ArgumentException("Balance cannot be negative");

        AccountId = accountId;
        Balance = balance;
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be > 0");

        Balance += amount;
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be > 0");

        if (Balance < amount)
            throw new InvalidOperationException("Insufficient balance");

        Balance -= amount;
    }
}