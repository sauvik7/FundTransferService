namespace FundTransfer.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public string RequestId { get; private set; } = null!;
    public string FromAccount { get; private set; } = null!;
    public string ToAccount { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Status { get; private set; } = null!;

    public Transaction(string requestId, string fromAccount, string toAccount, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("RequestId required");

        if (string.IsNullOrWhiteSpace(fromAccount) ||
            string.IsNullOrWhiteSpace(toAccount))
            throw new ArgumentException("Account details required");

        if (amount <= 0)
            throw new ArgumentException("Amount must be > 0");

        if (fromAccount == toAccount)
            throw new ArgumentException("Cannot transfer to same account");

        Id = Guid.NewGuid();
        RequestId = requestId;
        FromAccount = fromAccount;
        ToAccount = toAccount;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
        Status = "INITIATED";
    }

    public void MarkSuccess()
    {
        Status = "SUCCESS";
    }

    public void MarkFailed()
    {
        Status = "FAILED";
    }
}
