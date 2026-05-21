namespace FundTransfer.Application.Interfaces;

public interface IAccountStore
{
    bool TryGetBalance(string accountId, out decimal balance);
    void EnsureAccountExists(string accountId);
    void Transfer(string fromAccount, string toAccount, decimal amount, string requestId);
}
