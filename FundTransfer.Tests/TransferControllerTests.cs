using System.Text.Json;
using FundTransfer.API.Controllers;
using FundTransfer.Application.DTOs;
using FundTransfer.Application.Services;
using FundTransfer.Domain.Entities;
using FundTransfer.Domain.Services;
using FundTransfer.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FundTransfer.Tests;

public class TransferControllerTests
{
    private class TestOtpValidator : FundTransfer.Application.Interfaces.IOtpValidator
    {
        public bool Validate(string otp) => otp == "123456";
    }

    private class TestAuditLogger : FundTransfer.Application.Interfaces.IAuditLogger
    {
        public Task LogAsync(Transaction transaction, string outcome, string? error = null)
        {
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

        var acc1 = new Account("ACC1", 250000m);
        var acc2 = new Account("ACC2", 5000m);

        context.Accounts.AddRange(acc1, acc2);
        context.SaveChanges();

        return new TransferService(
            new EfAccountStore(context),
            new TestOtpValidator(),
            new InMemoryIdempotencyStore(),
            new SimpleThresholdFraudService(1000m),
            new TestAuditLogger(),
            new TransferDomainService() // ✅ REQUIRED
        );
    }

    [Fact]
    public async Task Transfer_ReturnsOk_WhenRequestIsValid()
    {
        // Arrange
        var service = CreateTransferService();
        var controller = new TransferController(service);
        var request = CreateRequest();

        // Act
        var result = await controller.Transfer(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);

        Assert.Contains("Transfer successful", json);
    }

    [Fact]
    public async Task Transfer_ReturnsBadRequest_WhenServiceFails()
    {
        // Arrange
        var service = CreateTransferService();
        var controller = new TransferController(service);
        var request = CreateRequest(requestId: "req-invalid-otp", otp: "000000");

        // Act
        var result = await controller.Transfer(request);

        // Assert
        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, problem.StatusCode);

        var json = JsonSerializer.Serialize(problem.Value);
        Assert.Contains("Invalid OTP", json);
    }

    [Fact]
    public async Task Transfer_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var service = CreateTransferService();
        var controller = new TransferController(service);
        controller.ModelState.AddModelError("FromAccount", "Required");

        // Act
        var result = await controller.Transfer(CreateRequest());

        // Assert
        Assert.IsType<ObjectResult>(result); // ValidationProblem returns ObjectResult
    }
}
