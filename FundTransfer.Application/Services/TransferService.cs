using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;
using FundTransfer.Domain.Services;

namespace FundTransfer.Application.Services;

public class TransferService(
    IAccountStore accountStore,
    IOtpValidator otpValidator,
    IIdempotencyStore idempotency,
    IFraudService fraud,
    IAuditLogger audit,
    TransferDomainService domainService)
{
    public async Task<(bool Success, string? Error)> ProcessAsync(TransferRequest request)
    {
        // ✅ Idempotency
        if (await idempotency.ExistsAsync(request.RequestId))
            return (false, "Duplicate request");

        // ✅ OTP validation
        if (!otpValidator.Validate(request.Otp))
            return (false, "Invalid OTP");

        // ✅ Fraud detection
        if (fraud.IsFraudulent(request, out var fraudReason))
            return (false, fraudReason);

        // ✅ Load accounts
        var from = await accountStore.GetAsync(request.FromAccount);
        if (from == null)
            return (false, "Source account not found");

        var existingTo = await accountStore.GetAsync(request.ToAccount);
        var to = existingTo ?? new Account(request.ToAccount, 0m);

        try
        {
            // ✅ All domain execution inside try
            var tx = domainService.Execute(
                from,
                to,
                request.RequestId,
                request.Amount);

            // ✅ Add destination account if newly created
            if (existingTo == null)
                await accountStore.AddAsync(to);

            // ✅ Persist changes
            await accountStore.SaveChangesAsync();

            // ✅ Mark idempotent AFTER commit
            await idempotency.MarkProcessedAsync(request.RequestId);

            // ✅ Audit success
            await audit.LogAsync(tx, "SUCCESS");

            return (true, null);
        }
        catch (ArgumentException ex)
        {
            await SafeAudit(request, ex);

            return (false, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await SafeAudit(request, ex);

            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            await SafeAudit(request, ex);

            throw; // ✅ preserve system errors
        }
    }

    // ✅ SAFE audit helper (never throws)
    private async Task SafeAudit(TransferRequest request, Exception ex)
    {
        try
        {
            var tx = new Transaction(
                request.RequestId,
                string.IsNullOrWhiteSpace(request.FromAccount) ? "INVALID" : request.FromAccount,
                string.IsNullOrWhiteSpace(request.ToAccount) ? "INVALID" : request.ToAccount,
                request.Amount > 0 ? request.Amount : 1 // ensure valid
            );

            await audit.LogAsync(tx, "FAILED", ex.Message);
        }
        catch
        {
            // ✅ Final fallback: NEVER let audit break business flow
        }
    }
}
