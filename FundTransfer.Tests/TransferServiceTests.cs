using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;
using FundTransfer.Application.Services;
using FundTransfer.Domain.Entities;
using FundTransfer.Domain.Services;
using FundTransfer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Moq;

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

        var (Success, Error) = await _service.ProcessAsync(request);
        var second = await _service.ProcessAsync(request);

        Assert.True(Success);
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

    [Fact]
    public async Task ProcessAsync_ReturnsFraud_WhenFraudDetected()
    {
        var fraudMock = new Mock<IFraudService>();

        fraudMock
            .Setup(x => x.IsFraudulent(It.IsAny<TransferRequest>(), out It.Ref<string>.IsAny!))
            .Returns((TransferRequest _, out string reason) =>
            {
                reason = "Fraud detected";
                return true;
            });

        var service = BuildService(fraudService: fraudMock.Object);

        var result = await service.ProcessAsync(ValidRequest());

        Assert.False(result.Success);
        Assert.Equal("Fraud detected", result.Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsError_WhenOtpInvalid()
    {
        var otp = new Mock<IOtpValidator>();
        otp.Setup(x => x.Validate(It.IsAny<string>())).Returns(false);

        var service = BuildService(otpValidator: otp.Object);

        var (Success, Error) = await service.ProcessAsync(ValidRequest());

        Assert.False(Success);
        Assert.Equal("Invalid OTP", Error);
    }

    [Fact]
    public async Task ProcessAsync_ReturnsDuplicate_WhenRequestExists()
    {
        var idem = new Mock<IIdempotencyStore>();
        idem.Setup(x => x.ExistsAsync("req1")).ReturnsAsync(true);

        var service = BuildService(idemStore: idem.Object);

        var (Success, Error) = await service.ProcessAsync(ValidRequest());

        Assert.False(Success);
        Assert.Equal("Duplicate request", Error);
    }

    private static TransferRequest ValidRequest()
    {
        return new TransferRequest
        {
            FromAccount = "ACC1",
            ToAccount = "ACC2",
            Amount = 100,
            RequestId = "req1",
            Otp = "123456"
        };
    }

    private static TransferService BuildService(
    IAccountStore? accountStore = null,
    IOtpValidator? otpValidator = null,
    IIdempotencyStore? idemStore = null,
    IFraudService? fraudService = null,
    IAuditLogger? auditLogger = null)
    {
        accountStore ??= new Mock<IAccountStore>().Object;

        otpValidator ??= Mock.Of<IOtpValidator>(
            x => x.Validate(It.IsAny<string>()) == true);

        idemStore ??= Mock.Of<IIdempotencyStore>(
            x => x.ExistsAsync(It.IsAny<string>()) == Task.FromResult(false));

        if (fraudService == null)
        {
            var fraudMock = new Mock<IFraudService>();

            fraudMock
                .Setup(x => x.IsFraudulent(It.IsAny<TransferRequest>(), out It.Ref<string>.IsAny!))
                .Returns((TransferRequest _, out string reason) =>
                {
                    reason = string.Empty;
                    return false;
                });

            fraudService = fraudMock.Object;
        }

        auditLogger ??= new Mock<IAuditLogger>().Object;

        var domainService = new TransferDomainService();

        return new TransferService(
            accountStore,
            otpValidator,
            idemStore,
            fraudService,
            auditLogger,
            domainService);
    }
}