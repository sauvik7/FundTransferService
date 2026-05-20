namespace FundTransfer.Application.Interfaces;

public interface IOtpValidator
{
    bool Validate(string otp);
}
