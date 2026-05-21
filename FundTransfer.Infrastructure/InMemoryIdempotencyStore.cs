using FundTransfer.Application.Interfaces;

namespace FundTransfer.Infrastructure;

public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly HashSet<string> _processed = new(StringComparer.OrdinalIgnoreCase);

    public bool IsProcessed(string requestId) => _processed.Contains(requestId);

    public void MarkProcessed(string requestId) => _processed.Add(requestId);
}
