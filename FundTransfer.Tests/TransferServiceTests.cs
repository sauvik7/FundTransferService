using FundTransfer.Application.DTOs;
using FundTransfer.Application.Services;
using FundTransfer.Domain.Entities;
using FundTransfer.Domain.Services;
using FundTransfer.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FundTransfer.Tests;

public class TransferServiceTests
{
    private readonly TransferService _service;

    public TransferServiceTests()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new PaymentsDbContext(options);

        // ✅ IMPORTANT FIX: lower balance so balance test works correctly
        var acc1 = new Account("ACC1", 500m);
        var acc2 = new Account("ACC2", 5000m);

        context.Accounts.AddRange(acc1, acc2);
        context.SaveChanges();

        _service = new TransferService(
            new EfAccountStore(context),
            new TestOtpValidator(),
            new InMemoryIdempotencyStore(),
            new SimpleThresholdFraudService(1000m),
            new TestAuditLogger(),
            new TransferDomainService()
        );
    }

    private class TestOtpValidator : FundTransfer.Application.Interfaces.IOtpValidator
    {
        public bool Validate(string otp) => otp == "123456";
    }

    private class TestAuditLogger : FundTransfer.Application.Interfaces.IAuditLogger
    {
        public List<string> Entries { get; } = new();

        public Task LogAsync(Transaction transaction, string outcome, string? error = null)
        {
            Entries.Add($"{transaction.RequestId}:{outcome}:{error}");
            return Task.CompletedTask;
        }
    }

    private static TransferRequest CreateRequest(
        string fromAccount = "ACC1",
        string toAccount = "ACC2",
        decimal amount = 500,
        string requestId = "req-success",
        string otp = "123456")
    {
        return new TransferRequest
        {
            FromAccount = fromAccount,
            ToAccount = toAccount,
            Amount = amount,
            RequestId = requestId,
            Otp = otp
        };
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSuccess_ForValidTransfer()
    {
        var request = CreateRequest(amount: 100);

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.True(Success);
        Assert.Null(Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsInvalidOtp_WhenOtpIsWrong()
    {
        var request = CreateRequest(requestId: "req-bad-otp", otp: "000000");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Invalid OTP", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsInsufficientBalance_WhenAmountExceedsBalance()
    {
        // ✅ amount < fraud threshold (1000) but > balance (500)
        var request = CreateRequest(amount: 999m, requestId: "req-low-balance");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Insufficient balance", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsDuplicateRequest_WhenSameRequestIdIsUsedTwice()
    {
        var request = CreateRequest(amount: 100, requestId: "req-duplicate");

        var first = await _service.ProcessAsync(request);
        var second = await _service.ProcessAsync(request);

        Assert.True(first.Success);
        Assert.False(second.Success);
        Assert.Equal("Duplicate request", second.Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsFraudError_WhenAmountExceedsThreshold()
    {
        var request = CreateRequest(amount: 2000, requestId: "req-fraud");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Contains("Amount exceeds allowed threshold", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSameAccountError_WhenFromAndToAreSame()
    {
        var request = CreateRequest(
            fromAccount: "ACC1",
            toAccount: "ACC1",
            requestId: "req-same-account");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Cannot transfer to same account", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsInvalidAmount_WhenAmountIsZero()
    {
        var request = CreateRequest(amount: 0, requestId: "req-invalid-amount");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Amount must be > 0", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSourceAccountNotFound_WhenFromAccountDoesNotExist()
    {
        var request = CreateRequest(
            fromAccount: "ACC999",
            toAccount: "ACC2",
            amount: 100,
            requestId: "req-source-not-found");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Source account not found", Error);
    }

    [Fact]
    public async Task ProcessAsync_CreatesNewDestinationAccount_WhenToAccountDoesNotExist()
    {
        var request = CreateRequest(
            fromAccount: "ACC1",
            toAccount: "ACC3",
            amount: 100,
            requestId: "req-new-destination");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.True(Success);
        Assert.Null(Error);
    }
}