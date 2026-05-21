using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;

namespace FundTransfer.Application.Services
{
    public class TransferService(
        IAccountStore accountStore,
        IOtpValidator otpValidator,
        IIdempotencyStore idempotencyStore,
        IFraudService fraudService,
        IAuditLogger auditLogger)
    {
        private readonly IAccountStore _accountStore = accountStore;
        private readonly IOtpValidator _otpValidator = otpValidator;
        private readonly IIdempotencyStore _idempotencyStore = idempotencyStore;
        private readonly IFraudService _fraudService = fraudService;
        private readonly IAuditLogger _auditLogger = auditLogger;

        public async Task<(bool Success, string? Error)> ProcessAsync(TransferRequest request)
        {
            // Defensive validation for domain invariants (keeps TransferService robust when called directly)
            if (string.IsNullOrWhiteSpace(request.FromAccount) || string.IsNullOrWhiteSpace(request.ToAccount))
            {
                return await LogFailureAsync(request, "Invalid account details");
            }

            if (string.Equals(request.FromAccount, request.ToAccount, StringComparison.OrdinalIgnoreCase))
            {
                return await LogFailureAsync(request, "Sender and receiver cannot be same");
            }

            if (request.Amount <= 0)
            {
                return await LogFailureAsync(request, "Amount must be greater than zero");
            }

            // Core responsibilities only: OTP verification, idempotency, business rules, and account operations.

            // OTP validation
            if (!_otpValidator.Validate(request.Otp))
            {
                return await LogFailureAsync(request, "Invalid OTP");
            }

            // Account existence
            if (!_accountStore.TryGetBalance(request.FromAccount, out decimal balance))
            {
                return await LogFailureAsync(request, "Source account not found");
            }

            // Balance validation
            if (balance < request.Amount)
            {
                return await LogFailureAsync(request, "Insufficient balance");
            }

            // Idempotency
            if (_idempotencyStore.IsProcessed(request.RequestId))
            {
                return await LogFailureAsync(request, "Duplicate request detected");
            }

            // Fraud check via injected service
            if (_fraudService.IsFraudulent(request, out var reason))
            {
                return await LogFailureAsync(request, reason ?? "Transaction flagged as fraudulent");
            }

            // Perform transfer
            _accountStore.EnsureAccountExists(request.ToAccount);
            try
            {
                _accountStore.Transfer(request.FromAccount, request.ToAccount, request.Amount);
                _idempotencyStore.MarkProcessed(request.RequestId);

                var transaction = new Transaction
                {
                    FromAccount = request.FromAccount,
                    ToAccount = request.ToAccount,
                    Amount = request.Amount,
                    RequestId = request.RequestId
                };

                await _auditLogger.LogAsync(transaction, "Success");
            }
            catch (Exception)
            {
                return await LogFailureAsync(request, "Internal error");
            }

            await Task.Delay(50);

            return (true, null);
        }

        private async Task<(bool Success, string? Error)> LogFailureAsync(TransferRequest request, string error)
        {
            var transaction = new Transaction
            {
                FromAccount = request.FromAccount,
                ToAccount = request.ToAccount,
                Amount = request.Amount,
                RequestId = request.RequestId
            };

            await _auditLogger.LogAsync(transaction, "Failure", error);
            return (false, error);
        }
    }
}