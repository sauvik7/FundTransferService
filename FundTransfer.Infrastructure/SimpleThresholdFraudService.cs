using FundTransfer.Application.DTOs;
using FundTransfer.Application.Interfaces;

namespace FundTransfer.Infrastructure;

public class SimpleThresholdFraudService(decimal threshold) : IFraudService
{
    public bool IsFraudulent(TransferRequest request, out string? reason)
    {
        if (request.Amount > threshold)
        {
            reason = $"Amount exceeds allowed threshold ({threshold})";
            return true;
        }

        reason = null;
        return false;
    }
}