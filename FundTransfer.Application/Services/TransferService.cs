using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;
using FundTransfer.Domain.Entities;

namespace FundTransfer.Application.Services
{
    public class TransferService(IAccountStore accountStore, IOtpValidator otpValidator)
    {
        private readonly IAccountStore _accountStore = accountStore;
        private readonly IOtpValidator _otpValidator = otpValidator;

        public async Task<(bool Success, string? Error)> ProcessAsync(TransferRequest request)
        {
            // ✅ 1. Basic validation
            if (string.IsNullOrWhiteSpace(request.FromAccount) ||
                string.IsNullOrWhiteSpace(request.ToAccount))
            {
                return (false, "Invalid account details");
            }

            if (request.FromAccount == request.ToAccount)
            {
                return (false, "Sender and receiver cannot be same");
            }

            if (request.Amount <= 0)
            {
                return (false, "Amount must be greater than zero");
            }

            // ✅ 2. OTP validation (simulated)
            if (!_otpValidator.Validate(request.Otp))
            {
                return (false, "Invalid OTP");
            }

            // ✅ 3. Account existence check
            if (!_accountStore.TryGetBalance(request.FromAccount, out decimal value))
            {
                return (false, "Source account not found");
            }

            // ✅ 4. Balance validation
            if (value < request.Amount)
            {
                return (false, "Insufficient balance");
            }

            // ✅ 5. Idempotency check (critical for retries)
            if (_accountStore.IsRequestProcessed(request.RequestId))
            {
                return (false, "Duplicate request detected");
            }

            // ✅ 6. Fraud / business rule
            if (request.Amount > 100000)
            {
                return (false, "Transaction limit exceeded");
            }

            // ✅ 7. Simulate transaction creation (domain usage)
            var transaction = new Transaction
            {
                FromAccount = request.FromAccount,
                ToAccount = request.ToAccount,
                Amount = request.Amount,
                RequestId = request.RequestId
            };

            _accountStore.EnsureAccountExists(request.ToAccount);
            _accountStore.Transfer(request.FromAccount, request.ToAccount, request.Amount);
            _accountStore.MarkRequestProcessed(request.RequestId);

            // ✅ Simulate async DB call
            await Task.Delay(50);

            return (true, null);
        }
    }
}