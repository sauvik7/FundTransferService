using FluentValidation;
using FundTransfer.Application.DTOs;

namespace FundTransfer.Application.Validators;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.FromAccount)
            .NotEmpty().WithMessage("FromAccount is required")
            .Matches("^[A-Z0-9]{1,32}$").WithMessage("FromAccount has invalid format");

        RuleFor(x => x.ToAccount)
            .NotEmpty().WithMessage("ToAccount is required")
            .Matches("^[A-Z0-9]{1,32}$").WithMessage("ToAccount has invalid format")
            .Must((request, toAccount) => !string.Equals(request.FromAccount, toAccount, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Sender and receiver cannot be same");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.RequestId)
            .NotEmpty().WithMessage("RequestId is required");

        RuleFor(x => x.Otp)
            .NotEmpty().WithMessage("Otp is required")
            .Length(4, 10).WithMessage("Otp length is invalid");
    }
}
