using System.Text.Json;
using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;

namespace FundTransfer.Infrastructure;

public class FileAuditLogger(string filePath) : IAuditLogger
{
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = false };

    public async Task LogAsync(Transaction transaction, string outcome, string? error = null)
    {
        var entry = new
        {
            Timestamp = DateTime.UtcNow,
            TransactionId = transaction.Id,
            FromAccount = transaction.FromAccount,
            ToAccount = transaction.ToAccount,
            transaction.Amount,
            transaction.RequestId,
            Outcome = outcome,
            Error = error
        };

        var line = JsonSerializer.Serialize(entry, _jsonOptions);
        // Ensure directory exists then append a single-line JSON entry to the file
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
    }
}
