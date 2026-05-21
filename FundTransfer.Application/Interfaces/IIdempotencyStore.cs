namespace FundTransfer.Application.Interfaces;

public interface IIdempotencyStore
{
    bool IsProcessed(string requestId);
    void MarkProcessed(string requestId);
}
