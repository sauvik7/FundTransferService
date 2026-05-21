namespace FundTransfer.Application.Interfaces;

public interface IIdempotencyStore
{
    Task<bool> ExistsAsync(string requestId);
    Task MarkProcessedAsync(string requestId);
}
