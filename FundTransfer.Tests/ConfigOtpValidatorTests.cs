using Microsoft.Extensions.Configuration;
using FundTransfer.Infrastructure;

namespace FundTransfer.Tests;

public class ConfigOtpValidatorTests
{
    private static IConfiguration BuildConfig(string otp)
    {
        var dict = new Dictionary<string, string?>
        {
            ["OtpSettings:Secret"] = otp
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [Fact]
    public void Validate_ReturnsTrue_WhenOtpMatches()
    {
        var config = BuildConfig("123456");
        var validator = new ConfigOtpValidator(config);

        var result = validator.Validate("123456");

        Assert.True(result);
    }

    [Fact]
    public void Validate_ReturnsFalse_WhenOtpDoesNotMatch()
    {
        var config = BuildConfig("123456");
        var validator = new ConfigOtpValidator(config);

        var result = validator.Validate("000000");

        Assert.False(result);
    }

    [Fact]
    public void Constructor_Throws_WhenOtpMissing()
    {
        var config = new ConfigurationBuilder().Build();

        Assert.Throws<InvalidOperationException>(() =>
            new ConfigOtpValidator(config));
    }
}