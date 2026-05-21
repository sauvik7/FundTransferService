using FundTransfer.Infrastructure;
using FundTransfer.Domain.Entities;

namespace FundTransfer.Tests;

public class FileAuditLoggerTests
{
    [Fact]
    public async Task LogAsync_WritesToFile()
    {
        var filePath = Path.GetTempFileName();
        var logger = new FileAuditLogger(filePath);

        var tx = new Transaction("req1", "A", "B", 100);
        tx.MarkSuccess();

        await logger.LogAsync(tx, "SUCCESS");

        var content = await File.ReadAllTextAsync(filePath);

        Assert.Contains("req1", content);
        Assert.Contains("SUCCESS", content);

        File.Delete(filePath);
    }
}
