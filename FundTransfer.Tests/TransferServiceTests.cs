using FundTransfer.Application.DTOs;
using FundTransfer.Application.Services;
using FundTransfer.Infrastructure;

namespace FundTransfer.Tests;

public class TransferServiceTests
{
    private readonly TransferService _service;

    public TransferServiceTests()
    {
        _service = new TransferService(new InMemoryAccountStore(), new TestOtpValidator());
    }

    private class TestOtpValidator : FundTransfer.Application.Interfaces.IOtpValidator
    {
        public bool Validate(string otp) => otp == "123456";
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
        var request = CreateRequest();

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
        var request = CreateRequest(amount: 250001, requestId: "req-low-balance");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Insufficient balance", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsDuplicateRequest_WhenSameRequestIdIsUsedTwice()
    {
        var request = CreateRequest(amount: 100, requestId: "req-duplicate");

        var (Success, Error) = await _service.ProcessAsync(request);
        var secondResult = await _service.ProcessAsync(request);

        Assert.True(Success);
        Assert.Null(Error);
        Assert.False(secondResult.Success);
        Assert.Equal("Duplicate request detected", secondResult.Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsTransactionLimitExceeded_WhenAmountIsAboveFraudThreshold()
    {
        var request = CreateRequest(amount: 200000, requestId: "req-fraud");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Transaction limit exceeded", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSameAccountError_WhenFromAndToAreSame()
    {
        var request = CreateRequest(fromAccount: "ACC1", toAccount: "ACC1", requestId: "req-same-account");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Sender and receiver cannot be same", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsInvalidAmount_WhenAmountIsZero()
    {
        var request = CreateRequest(amount: 0, requestId: "req-invalid-amount");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Amount must be greater than zero", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsInvalidAccountDetails_WhenFromAccountIsMissing()
    {
        var request = CreateRequest(fromAccount: string.Empty, requestId: "req-missing-account");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Invalid account details", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsSourceAccountNotFound_WhenFromAccountDoesNotExist()
    {
        var request = CreateRequest(fromAccount: "ACC999", toAccount: "ACC2", amount: 100, requestId: "req-source-not-found");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.False(Success);
        Assert.Equal("Source account not found", Error);
    }

    [Fact]
    public async Task ProcessAsync_CreatesNewDestinationAccount_WhenToAccountDoesNotExist()
    {
        var request = CreateRequest(fromAccount: "ACC1", toAccount: "ACC3", amount: 100, requestId: "req-new-destination");

        var (Success, Error) = await _service.ProcessAsync(request);

        Assert.True(Success);
        Assert.Null(Error);
    }
}