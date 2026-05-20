namespace FundTransfer.Application.Interfaces;

public interface IAccountStore
{
    bool TryGetBalance(string accountId, out decimal balance);
    bool IsRequestProcessed(string requestId);
    void MarkRequestProcessed(string requestId);
    void EnsureAccountExists(string accountId);
    void Transfer(string fromAccount, string toAccount, decimal amount);
}
