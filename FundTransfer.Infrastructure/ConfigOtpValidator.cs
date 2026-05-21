using FundTransfer.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FundTransfer.Infrastructure;

public class ConfigOtpValidator(IConfiguration configuration) : IOtpValidator
{
    private readonly string _secret = configuration["OtpSettings:Secret"]
                  ?? throw new InvalidOperationException("OTP secret not configured");

    public bool Validate(string otp)
    {
        return otp == _secret;
    }
}