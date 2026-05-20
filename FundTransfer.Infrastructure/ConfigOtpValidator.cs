using FundTransfer.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FundTransfer.Infrastructure;

public class ConfigOtpValidator(IConfiguration configuration) : IOtpValidator
{
    private readonly string _otpSecret = configuration["OtpSettings:Secret"]
                     ?? throw new InvalidOperationException("OtpSettings:Secret not configured");

    public bool Validate(string otp)
    {
        return otp == _otpSecret;
    }
}