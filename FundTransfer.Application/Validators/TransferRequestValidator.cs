using FluentValidation;
using FundTransfer.Application.DTOs;

namespace FundTransfer.Application.Validators;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.FromAccount)
            .NotEmpty();

        RuleFor(x => x.ToAccount)
            .NotEmpty();

        RuleFor(x => x.RequestId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Otp)
            .NotEmpty();
    }
}
