using System.Collections.Concurrent;
using FundTransfer.Application.Interfaces;

namespace FundTransfer.Infrastructure;

public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, byte> _store = new();

    public Task<bool> ExistsAsync(string requestId)
    {
        return Task.FromResult(_store.ContainsKey(requestId));
    }

    public Task MarkProcessedAsync(string requestId)
    {
        _store.TryAdd(requestId, 0);
        return Task.CompletedTask;
    }
}
