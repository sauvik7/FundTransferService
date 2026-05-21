using FundTransfer.Application.DTOs;
using FundTransfer.Application.Validators;

namespace FundTransfer.Tests;

public class TransferRequestValidatorTests
{
    private readonly TransferRequestValidator _validator = new();

    [Fact]
    public void Should_Fail_When_Accounts_Are_Same()
    {
        var req = new TransferRequest
        {
            FromAccount = "A",
            ToAccount = "A",
            Amount = 100,
            RequestId = "req1",
            Otp = "123"
        };

        var result = _validator.Validate(req);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Fail_When_Amount_Is_Negative()
    {
        var req = new TransferRequest
        {
            FromAccount = "A",
            ToAccount = "B",
            Amount = -10,
            RequestId = "req1",
            Otp = "123"
        };

        var result = _validator.Validate(req);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Should_Pass_For_Valid_Request()
    {
        var req = new TransferRequest
        {
            FromAccount = "A",
            ToAccount = "B",
            Amount = 100,
            RequestId = "req1",
            Otp = "123"
        };

        var result = _validator.Validate(req);

        Assert.True(result.IsValid);
    }
}