using FundTransfer.Domain.Entities;

namespace FundTransfer.Application.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(Transaction transaction, string outcome, string? error = null);
}
