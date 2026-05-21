using System.Text.Json;
using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;

namespace FundTransfer.Infrastructure;

public class FileAuditLogger(string filePath) : IAuditLogger
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false
    };

    public async Task LogAsync(Transaction transaction, string outcome, string? error = null)
    {
        var record = new
        {
            transaction.Id,
            transaction.RequestId,
            transaction.FromAccount,
            transaction.ToAccount,
            transaction.Amount,
            transaction.CreatedAt,
            Outcome = outcome,
            Error = error
        };

        var json = JsonSerializer.Serialize(record, _jsonOptions);

        await _lock.WaitAsync();
        try
        {
            await File.AppendAllTextAsync(filePath, json + Environment.NewLine);
        }
        finally
        {
            _lock.Release();
        }
    }
}