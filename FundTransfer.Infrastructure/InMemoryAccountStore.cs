using FundTransfer.Application.Interfaces;

namespace FundTransfer.Infrastructure;

public class InMemoryAccountStore : IAccountStore
{
    private readonly Dictionary<string, decimal> _balances = new(StringComparer.OrdinalIgnoreCase)
    {
        { "ACC1", 250000m },
        { "ACC2", 5000m }
    };

    private readonly HashSet<string> _processedRequests = new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetBalance(string accountId, out decimal balance) => _balances.TryGetValue(accountId, out balance);

    public bool IsRequestProcessed(string requestId) => _processedRequests.Contains(requestId);

    public void MarkRequestProcessed(string requestId) => _processedRequests.Add(requestId);

    public void EnsureAccountExists(string accountId)
    {
        if (!_balances.ContainsKey(accountId))
        {
            _balances[accountId] = 0m;
        }
    }

    public void Transfer(string fromAccount, string toAccount, decimal amount)
    {
        _balances[fromAccount] -= amount;
        EnsureAccountExists(toAccount);
        _balances[toAccount] += amount;
    }
}
