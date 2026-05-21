using FundTransfer.Application.DTOs;

namespace FundTransfer.Application.Interfaces;

public interface IFraudService
{
    bool IsFraudulent(TransferRequest request, out string? reason);
}
