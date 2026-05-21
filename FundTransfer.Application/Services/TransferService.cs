using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;

namespace FundTransfer.Application.Services
{
    public class TransferService(
        IAccountStore accountStore,
        IOtpValidator otpValidator,
        IIdempotencyStore idempotencyStore,
        IFraudService fraudService)
    {
        private readonly IAccountStore _accountStore = accountStore;
        private readonly IOtpValidator _otpValidator = otpValidator;
        private readonly IIdempotencyStore _idempotencyStore = idempotencyStore;
        private readonly IFraudService _fraudService = fraudService;

        public async Task<(bool Success, string? Error)> ProcessAsync(TransferRequest request)
        {
            // Defensive validation for domain invariants (keeps TransferService robust when called directly)
            if (string.IsNullOrWhiteSpace(request.FromAccount) || string.IsNullOrWhiteSpace(request.ToAccount))
            {
                return (false, "Invalid account details");
            }

            if (string.Equals(request.FromAccount, request.ToAccount, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Sender and receiver cannot be same");
            }

            if (request.Amount <= 0)
            {
                return (false, "Amount must be greater than zero");
            }

            // Core responsibilities only: OTP verification, idempotency, business rules, and account operations.

            // OTP validation
            if (!_otpValidator.Validate(request.Otp))
            {
                return (false, "Invalid OTP");
            }

            // Account existence
            if (!_accountStore.TryGetBalance(request.FromAccount, out decimal balance))
            {
                return (false, "Source account not found");
            }

            // Balance validation
            if (balance < request.Amount)
            {
                return (false, "Insufficient balance");
            }

            // Idempotency
            if (_idempotencyStore.IsProcessed(request.RequestId))
            {
                return (false, "Duplicate request detected");
            }

            // Fraud check via injected service
            if (_fraudService.IsFraudulent(request, out var reason))
            {
                return (false, reason);
            }

            // Perform transfer
            _accountStore.EnsureAccountExists(request.ToAccount);
            _accountStore.Transfer(request.FromAccount, request.ToAccount, request.Amount);
            _idempotencyStore.MarkProcessed(request.RequestId);

            await Task.Delay(50);

            return (true, null);
        }
    }
}