using System.Text.Json;
using FundTransfer.API.Controllers;
using FundTransfer.Application.DTOs;
using FundTransfer.Application.Services;
using FundTransfer.Domain.Entities;
using FundTransfer.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace FundTransfer.Tests;

public class TransferControllerTests
{
    public TransferControllerTests()
    {
    }

    private class TestOtpValidator : FundTransfer.Application.Interfaces.IOtpValidator
    {
        public bool Validate(string otp) => otp == "123456";
    }

    private class TestAuditLogger : FundTransfer.Application.Interfaces.IAuditLogger
    {
        public Task LogAsync(FundTransfer.Domain.Entities.Transaction transaction, string outcome, string? error = null)
        {
            // no-op for controller tests
            return Task.CompletedTask;
        }
    }

    private static TransferRequest CreateRequest(
        string fromAccount = "ACC1",
        string toAccount = "ACC2",
        decimal amount = 500,
        string requestId = "req-controller-success",
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

    private static TransferService CreateTransferService()
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new PaymentsDbContext(options);
        context.Database.EnsureCreated();
        context.Accounts.AddRange(
            new Account { AccountId = "ACC1", Balance = 250000m },
            new Account { AccountId = "ACC2", Balance = 5000m });
        context.SaveChanges();

        return new TransferService(
            new EfAccountStore(context),
            new TestOtpValidator(),
            new FundTransfer.Infrastructure.InMemoryIdempotencyStore(),
            new FundTransfer.Infrastructure.SimpleThresholdFraudService(1000m),
            new TestAuditLogger());
    }

    [Fact]
    public async Task Transfer_ReturnsOk_WhenRequestIsValid()
    {
        var service = CreateTransferService();
        var controller = new TransferController(service, NullLogger<TransferController>.Instance);
        var request = CreateRequest();

        var result = await controller.Transfer(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);

        Assert.Contains("Transfer successful", json);
    }

    [Fact]
    public async Task Transfer_ReturnsBadRequest_WhenServiceFails()
    {
        var service = CreateTransferService();
        var controller = new TransferController(service, NullLogger<TransferController>.Instance);
        var request = CreateRequest(requestId: "req-invalid-otp", otp: "000000");

        var result = await controller.Transfer(request);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        var json = JsonSerializer.Serialize(badResult.Value);

        Assert.Contains("Invalid OTP", json);
    }

    [Fact]
    public async Task Transfer_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var service = CreateTransferService();
        var controller = new TransferController(service, NullLogger<TransferController>.Instance);
        controller.ModelState.AddModelError("FromAccount", "Required");

        var result = await controller.Transfer(CreateRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
